using System;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Authenticators;

namespace RallyKnowledgeOwlIntegration
{
    class KnowledgeOwlDataService
    {
        public void UpdateBacklogArticle(List<dynamic> result)
        {
            //TODO pull out constants - URL, API key used to authenticate, and article ID (used in PUT)
            var knowledgeOwlRestClient = new RestClient("https://app.knowledgeowl.com/api/head/");
            knowledgeOwlRestClient.Authenticator = new HttpBasicAuthenticator("5616973c32131cf20f30cc56", "AnyFooBarPassword");
            if (knowledgeOwlRestClient == null) throw new ArgumentNullException(nameof(knowledgeOwlRestClient));

            //TODO Get verbaige from OCM to put at top of article
            var header = "{\"current_version\": {\"en\": {\"text\":\"<p>(**Placehoder text from OCM to describe the article.)</p><p><a href='%5B%5Bhg-id:57177b6632131cd43b6a2394%5D%5D' title='IT Prioritization'>IT Prioritization</a>&nbsp;</p><p></p><table align = 'center' border = '1' bordercolor = '#ccc' cellpadding = '5' cellspacing = '0' class='table table-small-font table-condensed table-bordered table-responsive' style='border-collapse:collapse;'><thead><tr><th scope = 'col'>ID</th><th scope='col'>Name&nbsp;</th><th scope = 'col' >Status&nbsp;</th><th scope = 'col'> Iteration</th><th scope='col'>Target Deployment Date</th></tr></thead><tbody>";
            var table = "";
            var footer = "</tbody></table>\"}}}";   

            //TODO Make sure Iteration, status and Release Date come back from Rally inside of result list                    
            foreach (var item in result)
            {                
                table += "<tr><td>" + item["FormattedID"] + "</td><td>" + item["Name"] + "</td><td>?Status?</td><td>" + item["Iteration"] +"</td><td>?Date?</td></tr>"; 
            }

            var body = header + table.Replace("\"", "'") + footer;            
            var urlPut = string.Format("article/{0}.json", "5717912932131c4f466a22e3");
            var requestPut = new RestRequest(urlPut, Method.PUT);
            requestPut.Parameters.Clear();
            requestPut.RequestFormat = DataFormat.Json;
            requestPut.AddHeader("accept", "application/json, text/plain, */*");
            requestPut.AddHeader("content-type", "application/json");                   
            requestPut.AddParameter("application/json", body, ParameterType.RequestBody);
            var responsePut = knowledgeOwlRestClient.Execute(requestPut);
            var response = responsePut.StatusCode;
        }
    }
}