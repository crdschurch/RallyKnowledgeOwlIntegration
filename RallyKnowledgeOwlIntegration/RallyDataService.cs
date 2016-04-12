using System;
using System.Collections.Generic;
using Rally.RestApi;
using Rally.RestApi.Response;

namespace RallyKnowledgeOwlIntegration
{
    public class RallyDataService
    {
        public QueryResult LoadStoriesAndDefects()
        {
            string apiKey = Environment.GetEnvironmentVariable("RALLY_API_KEY");
            string serverUrl = "https://rally1.rallydev.com";

            var restApi = new RallyRestApi();
            var authenticationResult = restApi.AuthenticateWithApiKey(apiKey, serverUrl);

            //Query for items
            var request = new Request("defect");
            request.Fetch = new List<string>() { "Name", "Description", "FormattedID" };
            request.Query = new Query("Name", Query.Operator.DoesNotContain, "My Defect");

            var queryResult = restApi.Query(request);
            return queryResult;
        }
    }
}
