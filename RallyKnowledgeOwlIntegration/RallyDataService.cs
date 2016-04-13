using System;
using System.Collections;
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
            var authenticationResult = restApi.AuthenticateWithApiKey(apiKey, serverUrl);

            //Query for items
            var results = new List<dynamic>();

            var stories = QueryArtifact(restApi, "SchedulableArtifact");
            results.AddRange(stories);

            return results;
        }

        private IEnumerable<dynamic> QueryArtifact(RallyRestApi restApi, string artifactType, int start = 1)
        {
            var request = new Request(artifactType);
            // TODO: Need iteration name, and rank (is this DragAndDropRank?)
            request.Fetch = new List<string>() {"Name", "FormattedID", "ScheduleState", "c_CrossroadsKanbanState", "Priority", "c_PriorityUS", "Iteration" };
                        
            // TODO: Needs cleanup, possibly use request.Query.And(), use constants for TypeDefOid
            var query = "((c_ProdSupportTeam = true) AND ((TypeDefOid = 22244455275) OR (TypeDefOid = 22244455200)))";
            request.Query = new Query(query);

            request.ProjectScopeDown = true;
            request.ProjectScopeUp = true;
            request.Start = start;

            var queryResult = restApi.Query(request);
            if (!queryResult.Success)
            {
                var errors = string.Join("\n", queryResult.Errors.Select(x => x.ToString()).ToList());
                var message = string.Format("Failed to query {0} from Rally due to errors: {1}", artifactType, errors);
                throw new Exception(message);
            }

            if (queryResult.TotalResultCount < queryResult.StartIndex + queryResult.Results.Count())
            {
                return queryResult.Results;
            }

            // TODO: Recursive call to get next batch of results
            var recursiveResults = QueryArtifact(restApi, artifactType, start + request.PageSize);

            return recursiveResults.Concat(queryResult.Results);            
        }
    }
}