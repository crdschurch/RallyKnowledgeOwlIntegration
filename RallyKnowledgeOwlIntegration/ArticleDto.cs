using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration
{
    class ArticleDto
    {   
        [JsonProperty(PropertyName = "current_version")]
        public ArticleCurrentVersion current_version { get; set; }

        public ArticleDto()
        {
            current_version = new ArticleCurrentVersion();
        }
    }
}
