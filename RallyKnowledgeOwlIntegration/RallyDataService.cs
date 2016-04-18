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
                        
            var iterations = QueryIterations(restApi);

            var results = QueryArtifact(restApi, iterations);
            return results.ToList();
        }

        private IEnumerable<string> QueryIterations(RallyRestApi restApi)
        {
            var request = new Request("Iterations");
            request.Fetch = new List<string>() { "Name" };

            var twoWeeksAgo = DateTime.Today.AddDays(-14).ToString("O");
            var dateQuery = new Query("EndDate", Query.Operator.GreaterThanOrEqualTo, twoWeeksAgo);
            var projectQuery = new Query("Project.Name", Query.Operator.Equals, "Ministry Platform");

            var completeQuery = dateQuery.And(projectQuery);
            request.Query = completeQuery;

            request.ProjectScopeDown = true;
            request.ProjectScopeUp = true;

            var results = LoadResults(restApi, request);
            var iterations = new List<string>();

            foreach (var result in results)
            {
                iterations.Add(result.Name);
            }

            return iterations;
        }

        private IEnumerable<dynamic> QueryArtifact(RallyRestApi restApi, IEnumerable<string> iterations)
        {
            var request = new Request("SchedulableArtifact");
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

        private Query GetDefectAndStoryQuery(IEnumerable<string> iterations)
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