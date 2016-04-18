using System;
using System.Collections.Generic;
using System.Linq;
using Rally.RestApi;

namespace RallyKnowledgeOwlIntegration
{
    public class RallyDataService
    {
        public List<dynamic> LoadStoriesAndDefects()
        {
            string apiKey = Environment.GetEnvironmentVariable("RALLY_API_KEY");
            string serverUrl = "https://rally1.rallydev.com";

            var restApi = new RallyRestApi();
            restApi.AuthenticateWithApiKey(apiKey, serverUrl);

            // TODO: Get Current and Previous Sprints from Rally
            var sprints = new List<string> {"S1", "S2"};
            
            var results = QueryArtifact(restApi, "SchedulableArtifact", sprints);            
            return results.ToList();
        }

        private IEnumerable<dynamic> QueryArtifact(RallyRestApi restApi, string artifactType, List<string> iterations, int start = 1)
        {
            var request = new Request(artifactType);
            // TODO: Need iteration name, and rank (is this DragAndDropRank?)
            request.Fetch = new List<string>() {"Name", "FormattedID", "ScheduleState", "c_CrossroadsKanbanState", "Priority", "c_PriorityUS", "Iteration" };

            request.Query = GetDefectAndStoryQuery(iterations);

            request.ProjectScopeDown = true;
            request.ProjectScopeUp = true;

            var results = LoadResults(restApi, request, 1);
            return results;
        }

        public IEnumerable<dynamic> LoadResults(RallyRestApi restApi, Request request, int start = 1)
        {
            Console.WriteLine("Running query {0}", request.Query.QueryClause);

            request.Start = start;
            var queryResult = restApi.Query(request);
            Console.WriteLine("Query returned {0} entries", queryResult.TotalResultCount);

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

        private static Query GetDefectAndStoryQuery(List<string> iterations)
        {
            var defectQuery = new Query("TypeDefOid", Query.Operator.Equals, "22244455275");
            var userStoriesQuery = new Query("TypeDefOid", Query.Operator.Equals, "22244455200");

            var prodSupportQuery = new Query("c_ProdSupportTeam", Query.Operator.Equals, "true");

            var allIterations = new Query("Iteration.Name", Query.Operator.Equals, "");
            foreach (var iteration in iterations)
            {
                var sprintsQuery = new Query("Iteration", Query.Operator.Equals, iteration);
                allIterations.Or(sprintsQuery);
            }

            var backlogQuery = defectQuery.Or(userStoriesQuery);
            var completeQuery = prodSupportQuery.And(backlogQuery).And(allIterations);
            return completeQuery;
        }
    }
}