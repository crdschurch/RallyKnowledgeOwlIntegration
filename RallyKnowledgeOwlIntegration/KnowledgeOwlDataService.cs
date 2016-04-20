using System;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Authenticators;

namespace RallyKnowledgeOwlIntegration
{
    class KnowledgeOwlDataService
    {
        public void UpdateBacklogArticle(IEnumerable<RallyArtifact> result)
        {
            //TODO pull out constants - URL, API key used to authenticate, and article ID (used in PUT)
            var knowledgeOwlRestClient = new RestClient("https://app.knowledgeowl.com/api/head/");
            knowledgeOwlRestClient.Authenticator = new HttpBasicAuthenticator("5616973c32131cf20f30cc56", "AnyFooBarPassword");
            if (knowledgeOwlRestClient == null) throw new ArgumentNullException(nameof(knowledgeOwlRestClient));

            //TODO Get verbaige from OCM to put at top of article
            var header = "{\"current_version\":\"<p>SARA TESTING NEW DEVELOPMENT (Placehoder text from OCM to describe the article.)</p><p></p><table align = 'center' border = '1' bordercolor = '#ccc' cellpadding = '5' cellspacing = '0' class='table table-small-font table-condensed table-bordered table-responsive' style='border-collapse:collapse;'><thead><tr><th scope = 'col'>ID</th><th scope='col'>Name&nbsp;</th><th scope = 'col' >Status&nbsp;</th><th scope = 'col'> Iteration</th><th scope='col'>Target Deployment Date</th></tr></thead><tbody>";
            var table = "";
            var footer = "</tbody></table>\"}";   

            //TODO Make sure Iteration, status and Release Date come back from Rally inside of result list                    
            foreach (var item in result)
            {
                var iterationName = item.Iteration != null ? item.Iteration.Name : string.Empty;
                table += "<tr><td>" + item.FormattedID + "</td><td>" + item.Name + "</td><td>" + item.Status + "</td><td>" + iterationName + "</td><td>?Date?</td></tr>"; 
            }
            var body = header + table.Replace("\"", "'") + footer;
            
            var urlPut = string.Format("article/{0}.json", "5715678132131c081e6a242a");
            var requestPut = new RestRequest(urlPut, Method.PUT);
            requestPut.RequestFormat = DataFormat.Json;
            requestPut.AddParameter("application/json", body, ParameterType.RequestBody);
            knowledgeOwlRestClient.Execute(requestPut);
        }
    }
}