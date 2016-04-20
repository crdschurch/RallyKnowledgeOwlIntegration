using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Rally.RestApi;
using RallyKnowledgeOwlIntegration.Helpers;

namespace RallyKnowledgeOwlIntegration
{
    public class RallyDataService
    {
        public IList<RallyArtifact> LoadStoriesAndDefects()
        {
            string apiKey = Environment.GetEnvironmentVariable("RALLY_API_KEY");
            string serverUrl = "https://rally1.rallydev.com";

            var restApi = new RallyRestApi();
            restApi.AuthenticateWithApiKey(apiKey, serverUrl);

            var iterations = QueryIterations(restApi);
            var artifacts = QueryArtifact(restApi, iterations);

            ProcessResults(artifacts, iterations);
            return artifacts;
        }

        private IList<RallyIteration> QueryIterations(RallyRestApi restApi)
        {
            var request = new Request("Iterations");
            request.Fetch = new List<string>() { "Name", "EndDate" };            

            var twoWeeksAgo = DateTime.Today.AddDays(-14).ToString("O");
            var dateQuery = new Query("EndDate", Query.Operator.GreaterThanOrEqualTo, twoWeeksAgo);
            var projectQuery = new Query("Project.Name", Query.Operator.Equals, "Ministry Platform");

            var completeQuery = dateQuery.And(projectQuery);
            request.Query = completeQuery;

            request.ProjectScopeDown = true;
            request.ProjectScopeUp = true;

            var results = RallyApiHelper.LoadResults(restApi, request);
            var iterations = AutoMapperHelper.MapToListObjects<RallyIteration>(results);

            return iterations;
        }

        private IList<RallyArtifact> QueryArtifact(RallyRestApi restApi, IEnumerable<RallyIteration> iterations)
        {
            var request = new Request("SchedulableArtifact");
            // TODO: Need iteration name, and rank (is this DragAndDropRank?)
            request.Fetch = new List<string>() { "Name", "FormattedID", "ScheduleState", "c_CrossroadsKanbanState", "Priority", "c_PriorityUS", "Iteration" };
            request.Query = GetDefectAndStoryQuery(iterations);

            request.ProjectScopeDown = true;
            request.ProjectScopeUp = true;

            var results = RallyApiHelper.LoadResults(restApi, request);
            var artifacts = AutoMapperHelper.MapToListObjects<RallyArtifact>(results);
            return artifacts;
        }

        private void ProcessResults(IList<RallyArtifact> results, IList<RallyIteration> iterations)
        {
            foreach (var result in results)
            {
                result.Status = MapStatus(result, iterations);
            }
        }

        private string MapStatus(RallyArtifact artifact, IList<RallyIteration> iterations)
        {
            var state = (string) artifact.c_CrossroadsKanbanState;
            switch (state)
            {
                case "Developing":
                case "Code Review":
                case "Testing":
                case "Acceptance Review":
                    return "In-Progress";
                    
                case "Done":
                    // TODO: What should we do if iteration is null, it would be in this list always
                    if (artifact.Iteration == null)
                    {
                        return "Deployed";
                    }

                    var iteration = iterations.FirstOrDefault(x => x.Name == artifact.Iteration.Name);
                    if (iteration == null)
                    {
                        return "Deployed";
                    }

                    // TODO: Does this need to be EndDate + deploymentOffset?
                    if (iteration.EndDate < DateTime.Today)
                    {
                        return "Deployed";
                    }

                    return "Completed";

                default:
                    return "Backlog";
            }
        }

        private Query GetDefectAndStoryQuery(IEnumerable<RallyIteration> iterations)
        {
            var defectQuery = new Query("TypeDefOid", Query.Operator.Equals, "22244455275");
            var userStoriesQuery = new Query("TypeDefOid", Query.Operator.Equals, "22244455200");

            var prodSupportQuery = new Query("c_ProdSupportTeam", Query.Operator.Equals, "true");

            var allIterations = new Query("Iteration.Name", Query.Operator.Equals, "");
            foreach (var iteration in iterations)
            {
                var sprintsQuery = new Query("Iteration.Name", Query.Operator.Equals, iteration.Name);
                allIterations = allIterations.Or(sprintsQuery);
            }

            var backlogQuery = defectQuery.Or(userStoriesQuery);
            var completeQuery = prodSupportQuery.And(backlogQuery).And(allIterations);
            return completeQuery;
        }
    }
}