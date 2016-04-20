using System;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Authenticators;
using log4net;
using RallyKnowledgeOwlIntegration.Models;

namespace RallyKnowledgeOwlIntegration
{
    class KnowledgeOwlDataService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(KnowledgeOwlDataService));

        public void UpdateBacklogArticle(RallyArtifactsByState artifactsByState)
        {            
            string apiKey = Environment.GetEnvironmentVariable("KNOWLEDGE_OWL_API_KEY");
            string articleId = Environment.GetEnvironmentVariable("KNOWLEDGE_OWL_ARTICLE_ID");
            
            var knowledgeOwlRestClient = new RestClient("https://app.knowledgeowl.com/api/head/");
            knowledgeOwlRestClient.Authenticator = new HttpBasicAuthenticator(apiKey, "AnyFooBarPassword");
            if (knowledgeOwlRestClient == null) throw new ArgumentNullException(nameof(knowledgeOwlRestClient));

            //TODO Get verbaige from OCM to put at top of article            
            var header = "{\"current_version\": {\"en\": {\"text\":\"<p>(**Placehoder text from OCM to describe the article.)</p><p><a href='%5B%5Bhg-id:57177b6632131cd43b6a2394%5D%5D' title='IT Prioritization'>IT Prioritization</a>&nbsp;</p><p></p>";
            var footer = "\"}}}";

            var backlogTable = CreateTable(artifactsByState.Backlog);
            var currentTable = CreateTable(artifactsByState.CurrentIteration);
            var previousTable = CreateTable(artifactsByState.PreviousIterations);
            var body = header + currentTable + "<br/>" + previousTable + backlogTable + "<br/>"  + footer;

            var urlPut = string.Format("article/{0}.json", articleId);
            var requestPut = new RestRequest(urlPut, Method.PUT);
            requestPut.Parameters.Clear();
            requestPut.RequestFormat = DataFormat.Json;
            requestPut.AddHeader("accept", "application/json, text/plain, */*");
            requestPut.AddHeader("content-type", "application/json");                   
            requestPut.AddParameter("application/json", body, ParameterType.RequestBody);
            var responsePut = knowledgeOwlRestClient.Execute(requestPut);

            //TODO error handling/logging - need to log content results
            var content = responsePut.Content;
            var response = responsePut.StatusCode;
            _logger.Debug(content);
        }

        private static string CreateTable(List<RallyArtifact> artifacts)
        {
            var tableHeader =
                "<table align = 'center' border = '1' bordercolor = '#ccc' cellpadding = '5' cellspacing = '0' class='table table-small-font table-condensed table-bordered table-responsive' style='border-collapse:collapse;'><thead><tr><th scope = 'col'>ID</th><th scope='col'>Name&nbsp;</th><th scope = 'col' >Status&nbsp;</th><th scope = 'col'> Iteration</th><th scope='col'>Target Deployment Date</th></tr></thead><tbody>";

            var table = "";
            var tableFooter = "</tbody></table>";            

            //TODO Make sure Iteration, status and Release Date come back from Rally inside of result list                    
            foreach (var item in artifacts)
            {                
                table += "<tr><td>" + item.FormattedId + "</td><td>" + item.Name + "</td><td>" + item.Status + "</td><td>" +
                         item.IterationName + "</td><td>?Date?</td></tr>";
            }

            table = tableHeader + table.Replace("\"", "'") + tableFooter;
            return table;
        }
    }
}