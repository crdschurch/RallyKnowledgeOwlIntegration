using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace RallyKnowledgeOwlIntegration
{
    class KnowledgeOwlDataService
    {

        public void UpdateBacklogArticle(List<dynamic> result)
        {
            //TODO pull out constants
            var knowledgeOwlRestClient = new RestClient("https://app.knowledgeowl.com/api/head/");
            knowledgeOwlRestClient.Authenticator = new HttpBasicAuthenticator("5616973c32131cf20f30cc56", "AnyFooBarPassword");
            if (knowledgeOwlRestClient == null) throw new ArgumentNullException(nameof(knowledgeOwlRestClient));

            var urlGet = string.Format("article/{0}.json", "570ff4c732131cdb44804aea");
            var requestGet = new RestRequest(urlGet, Method.GET);
            requestGet.AddParameter("project_id", "55d778fd32131c204151f217");

            var responseGet = knowledgeOwlRestClient.Execute(requestGet);
            //var article = knowledgeOwlRestClient.Execute<ArticleDto>(requestGet);

            //TODO JSON mapping not working??
            var article = new ArticleDto();
            //JsonConvert.PopulateObject(responseGet.Content, article);
            article = JsonConvert.DeserializeObject<ArticleDto>(responseGet.Content);

            //TODO build html article html table for article body from Rally results
            article.CurrentVersion = "newly built html string using rally result List";

            //TODO update exisitng article (current_version/text field) with newly built html string                  
            // PUT https://app.knowledgeowl.com/api/head/article/570ff4c732131cdb44804aea.json
            /* {
                "current_version":
                "<p>THIS IS AN UPDATED TEST - Body of article.</p>"                
               }            
            */
            //var urlPut = string.Format("article/{0}.json", "570ff4c732131cdb44804aea");
            //var requestPut = new RestRequest(urlPut, Method.PUT);
            //requestPut.AddParameter("project_id", "55d778fd32131c204151f217");
            //add other parameters/body
            //var responsePut = knowledgeOwlRestClient.Execute(requestPut);
        }
    }
}
