using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Rally.RestApi;

namespace RallyKnowledgeOwlIntegration
{
    public class RallyApiHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RallyApiHelper));
        public static IEnumerable<dynamic> LoadResults(RallyRestApi restApi, Request request, int start = 1)
        {
            Logger.InfoFormat("Running query {0} starting at index {1}", request.Query.QueryClause, start);

            request.Start = start;
            var queryResult = restApi.Query(request);
            Logger.InfoFormat("Query has {0} total matches, this batch has {1} items", queryResult.TotalResultCount, queryResult.Results.Count());

            if (!queryResult.Success)
            {
                var errors = string.Join("\n", queryResult.Errors.Select(x => x.ToString()).ToList());
                var message = string.Format("Failed to query Rally due to errors: {0}", errors);
                Logger.Error(message);
                throw new Exception(message);
            }

            var currentResults = queryResult.Results.ToList();
            if (queryResult.TotalResultCount < queryResult.StartIndex + currentResults.Count())
            {
                return currentResults;
            }

            // Recursive call to get next batch of results
            var recursiveResults = LoadResults(restApi, request, start + request.PageSize);

            return currentResults.Concat(recursiveResults);
        }
    }
}