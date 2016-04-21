using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration
{
    class ArticleCurrentVersion
    {
        //knowledge owl crossroads/ministry platform project id
        [JsonProperty(PropertyName = "en")]
        public ArticleLanguage Language { get; set; }
    }
}
