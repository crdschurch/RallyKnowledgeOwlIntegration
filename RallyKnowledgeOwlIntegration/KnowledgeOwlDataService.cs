using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using RestSharp.Authenticators;
using log4net;

namespace RallyKnowledgeOwlIntegration
{
    class KnowledgeOwlDataService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(KnowledgeOwlDataService));

        public void UpdateBacklogArticle(IEnumerable<RallyArtifact> result)
        {            
            string apiKey = Environment.GetEnvironmentVariable("KNOWLEDGE_OWL_API_KEY");
            string articleId = Environment.GetEnvironmentVariable("KNOWLEDGE_OWL_ARTICLE_ID");
            
            var knowledgeOwlRestClient = new RestClient("https://app.knowledgeowl.com/api/head/");
            knowledgeOwlRestClient.Authenticator = new HttpBasicAuthenticator(apiKey, "AnyFooBarPassword");
            if (knowledgeOwlRestClient == null) throw new ArgumentNullException(nameof(knowledgeOwlRestClient));

            //TODO Get verbaige from OCM to put at top of article
            var header = "{\"current_version\": {\"en\": {\"text\":\"<p><a href='%5B%5Bhg-id:57177b6632131cd43b6a2394%5D%5D' title='IT Prioritization'>IT Prioritization</a>&nbsp;</p><p>(Placehoder text from OCM to describe the article.)</p><p></p><table align = 'center' border = '1' bordercolor = '#ccc' cellpadding = '5' cellspacing = '0' class='table table-small-font table-condensed table-bordered table-responsive' style='border-collapse:collapse;'><thead><tr><th scope = 'col'>ID</th><th scope='col'>Name&nbsp;</th><th scope = 'col' >Status&nbsp;</th><th scope = 'col'> Iteration</th><th scope='col'>Target Deployment Date</th></tr></thead><tbody>";
            var table = "";
            var footer = "</tbody></table>\"}}}";   

            //TODO Break into 3 sections - previous sprint, current sprint, backlog                  
            foreach (var item in result)
            {
                var iterationName = item.Iteration != null ? item.Iteration.Name : string.Empty;
                table += "<tr><td style='width: 100px;'>" + item.FormattedID + "</td><td style='width: 250px;'>" + item.Name + "</td><td style='width: 150px;'>" + item.Status + "</td><td style='width: 100px;'>" + iterationName + "</td><td style='width: 150px;'>?Date?</td></tr>"; 
            }

            var body = header + table.Replace("\"", "'") + footer;            
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
    }
}