using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration.Models
{
    public class ArticleLanguages
    {
        [JsonProperty(PropertyName = "en")]
        public ArticleBody en { get; set; }

        public ArticleLanguages()
        {
            en = new ArticleBody();
        }
    }
}
