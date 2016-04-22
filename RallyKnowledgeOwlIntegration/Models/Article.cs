using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration.Models
{
    public class Article
    {   
        [JsonProperty(PropertyName = "current_version")]
        public ArticleLanguages current_version { get; set; }

        public Article()
        {
            current_version = new ArticleLanguages();
        }
    }
}
