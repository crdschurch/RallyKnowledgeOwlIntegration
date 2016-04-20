using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Rally.RestApi;

namespace RallyKnowledgeOwlIntegration
{
    public class RallyDataService
    {
        public IEnumerable<RallyArtifact> LoadStoriesAndDefects()
        {
            string apiKey = Environment.GetEnvironmentVariable("RALLY_API_KEY");
            string serverUrl = "https://rally1.rallydev.com";

            var restApi = new RallyRestApi();
            restApi.AuthenticateWithApiKey(apiKey, serverUrl);

            var iterations = QueryIterations(restApi);
            var artifacts = QueryArtifact(restApi, iterations);

            processResults(artifacts, iterations);
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

            var results = LoadResults(restApi, request);
            var iterations = MapToListObjects<RallyIteration>(results);

            return iterations;
        }

        private static List<T> MapToListObjects<T>(IEnumerable<dynamic> results)
        {
            var iterations = new List<T>();

            var config = new MapperConfiguration(cfg => { });
            var mapper = config.CreateMapper();

            foreach (var result in results)
            {
                var iteration = mapper.Map<T>(result);
                iterations.Add(iteration);
            }
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

            var results = LoadResults(restApi, request, 1);

            var artifacts = MapToListObjects<RallyArtifact>(results);
            return artifacts;
        }

        private void processResults(IEnumerable<RallyArtifact> results, IEnumerable<RallyIteration> iterations)
        {
            foreach (var result in results)
            {
                result.Status = MapStatus(result, iterations);
            }
        }

        private string MapStatus(RallyArtifact artifact, IEnumerable<RallyIteration> iterations)
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
                    // TODO: What should we if iteration is null, it would be in this list always
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

        public IEnumerable<dynamic> LoadResults(RallyRestApi restApi, Request request, int start = 1)
        {
            Console.WriteLine("Running query {0} starting at {1}", request.Query.QueryClause, start);

            request.Start = start;
            var queryResult = restApi.Query(request);
            Console.WriteLine("Query has {0} total matches, this batch has {1} entries", queryResult.TotalResultCount, queryResult.Results.Count());

            if (!queryResult.Success)
            {
                var errors = string.Join("\n", queryResult.Errors.Select(x => x.ToString()).ToList());
                var message = string.Format("Failed to query Rally due to errors: {1}", errors);
                throw new Exception(message);
            }

            var currentResults = queryResult.Results.ToList();
            if (queryResult.TotalResultCount < queryResult.StartIndex + currentResults.Count())
            {
                return currentResults;
            }

            // TODO: Recursive call to get next batch of results
            var recursiveResults = LoadResults(restApi, request, start + request.PageSize);

            return currentResults.Concat(recursiveResults);
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