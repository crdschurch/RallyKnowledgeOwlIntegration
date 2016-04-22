using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration
{
    class ArticleLanguage
    {
        [JsonProperty(PropertyName = "text")]
        public string text { get; set; }
    }
}
