using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration.Models
{
    public class ArticleBody
    {
        [JsonProperty(PropertyName = "text")]
        public string text { get; set; }
    }
}
