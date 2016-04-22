using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration.Models
{
    class ArticleCurrentVersion
    {
        [JsonProperty(PropertyName = "en")]
        public ArticleLanguage en { get; set; }

        public ArticleCurrentVersion()
        {
            en = new ArticleLanguage();
        }
    }
}
