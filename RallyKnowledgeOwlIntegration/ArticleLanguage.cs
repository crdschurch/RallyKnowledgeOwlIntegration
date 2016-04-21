using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration
{
    class ArticleLanguage
    {
        //knowledge owl crossroads/ministry platform project id
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
