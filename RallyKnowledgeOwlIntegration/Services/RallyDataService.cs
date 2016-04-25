using System;
using System.Collections.Generic;
using System.Linq;
using Rally.RestApi;
using RallyKnowledgeOwlIntegration.Helpers;
using RallyKnowledgeOwlIntegration.Models;

namespace RallyKnowledgeOwlIntegration.Services
{
    public class RallyDataService
    {
        private const int DeploymentOffsetInDays = 3;

        public RallyArtifactsByState LoadArtifactsByState()
        {
            string apiKey = Environment.GetEnvironmentVariable("RALLY_API_KEY");
            string serverUrl = "https://rally1.rallydev.com";

            var restApi = new RallyRestApi();
            restApi.AuthenticateWithApiKey(apiKey, serverUrl);

            var iterations = QueryIterations(restApi);
            var artifacts = QueryArtifact(restApi, iterations);

            ProcessResults(artifacts, iterations);

            var artifactsByState = GroupByStates(artifacts, iterations);
            return artifactsByState;
        }

        private RallyArtifactsByState GroupByStates(IList<RallyArtifact> artifacts, IList<RallyIteration> iterations)
        {
            var artifactsByState = new RallyArtifactsByState();
            
            var previous = iterations.Where(x => x.EndDate < DateTime.Today).Select(x => x.Name);
            artifactsByState.PreviousIterations = artifacts.Where(x => previous.Contains(x.IterationName)).ToList();
            
            var current =
                iterations.Where(x => x.EndDate >= DateTime.Today &&
                                      x.StartDate <= DateTime.Today).Select(x => x.Name);

            artifactsByState.CurrentIteration =
                artifacts.Where(x => current.Contains(x.IterationName)).ToList();

            artifactsByState.Backlog =
                artifacts.Where(x => previous.Contains(x.IterationName) == false &&
                                     current.Contains(x.IterationName) == false)
                    .ToList();

            return artifactsByState;
        }

        private IList<RallyIteration> QueryIterations(RallyRestApi restApi)
        {
            var request = new Request("Iterations");
            request.Fetch = new List<string>() {"Name", "EndDate", "StartDate"};

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
            var request = new Request("Artifact");
            request.Fetch = new List<string>() { "Name", "FormattedID", "ScheduleState", "c_CrossroadsKanbanState", "Priority", "c_PriorityUS", "Iteration", "DragAndDropRank"};
            request.Query = GetDefectAndStoryQuery(iterations);
            request.AddParameter("types", "hierarchicalrequirement,defect");
            request.Order = "DragAndDropRank";

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
                var iteration = GetIteration(result, iterations);
                result.Status = CalculateStatus(result, iteration);
                result.TargetDate = CalculateTargetDate(result, iteration);
                
            }
        }

        private DateTime? CalculateTargetDate(RallyArtifact result, RallyIteration iteration)
        {
            if (result.Status == "Backlog")
            {
                return null;
            }

            if (iteration == null)
            {
                return null;
            }

            if (iteration.StartDate <= DateTime.Today)
            {
                return iteration.EndDate.AddDays(DeploymentOffsetInDays);
            }

            return null;
        }

        private RallyIteration GetIteration(RallyArtifact artifact, IList<RallyIteration> iterations)
        {
            if (artifact.IterationName == null)
            {
                return null;
            }

            var iteration = iterations.FirstOrDefault(x => x.Name == artifact.IterationName);
            return iteration;            
        }

        private string CalculateStatus(RallyArtifact artifact, RallyIteration iteration)
        {
            var state = (string) artifact.KanbanState;
            switch (state)
            {
                case "Developing":
                case "Code Review":
                case "Testing":
                case "Acceptance Review":
                    return "In-Progress";
                    
                case "Done":
                    if (iteration == null)
                    {
                        return "Deployed";
                    }

                    if (iteration.EndDate.AddDays(DeploymentOffsetInDays) <= DateTime.Today)
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
            var prodSupportQuery = new Query("c_ProdSupportTeam", Query.Operator.Equals, "true");

            var unscheduledIterationsQuery = new Query("Iteration.Name", Query.Operator.Equals, "");
            var notAcceptedQuery = new Query("ScheduleState", Query.Operator.DoesNotEqual, "Accepted");
            var notCompletedQuery = new Query("ScheduleState", Query.Operator.DoesNotEqual, "Completed");

            var notDefectQuery = new Query("TypeDefOid", Query.Operator.DoesNotEqual, "22244455275");
            var notClosedQuery = new Query("State", Query.Operator.DoesNotEqual, "Closed");
            var notClosedDefectQuery = notDefectQuery.Or(notClosedQuery);

            var allIterations = unscheduledIterationsQuery.And(notAcceptedQuery).And(notCompletedQuery).And(notClosedDefectQuery);

            foreach (var iteration in iterations)
            {
                var sprintsQuery = new Query("Iteration.Name", Query.Operator.Equals, iteration.Name);
                allIterations = allIterations.Or(sprintsQuery);
            }

            var completeQuery = prodSupportQuery.And(allIterations);
            return completeQuery;
        }
    }
}