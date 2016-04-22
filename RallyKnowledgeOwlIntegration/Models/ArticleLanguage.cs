using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration.Models
{
    class ArticleLanguage
    {
        [JsonProperty(PropertyName = "text")]
        public string text { get; set; }
    }
}
