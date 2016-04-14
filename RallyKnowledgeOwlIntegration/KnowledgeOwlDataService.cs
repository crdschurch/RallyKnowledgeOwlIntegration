using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RallyKnowledgeOwlIntegration
{
    class KnowledgeOwlDataService
    {
        public void UpdateBacklogArticle(List<dynamic> result)
        {
            //TODO connect to Knowledge Owl
            // KO API key = 5616973c32131cf20f30cc56
            // Passed via HTTP Basic Authentication in the username field
            // Any dummy password, can be used for the password field
            // project_id = 55d778fd32131c204151f217

            //TODO pull out constant - Article ID
            UpdateBacklogArticle("570fc7b291121c14364aeb65", result);
        }

        public void UpdateBacklogArticle(string articleId, List<dynamic> result)
        {
            //TODO load existing System Development Backlog article from knowledge owl
            // GET https://app.knowledgeowl.com/api/head/article/570ff4c732131cdb44804aea.json
            var urlGet = string.Format("https://app.knowledgeowl.com/api/head/article/{0}.json", articleId);
            
            ArticleDto articleDto = new ArticleDto();

            //TODO build html article body from Rally results
            articleDto.CurrentVersion = "newly built html string using rally result List";

            //TODO update exisitng article body html (current_version field) with newly built html string                  
            // PUT https://app.knowledgeowl.com/api/head/article/570ff4c732131cdb44804aea.json
            /* {
                "current_version":
                "<p>THIS IS AN UPDATED TEST - Body of article.</p>"                
               }            
            */
            var urlPut = string.Format("https://app.knowledgeowl.com/api/head/article/{0}.json", articleId);

        }
    }
}
