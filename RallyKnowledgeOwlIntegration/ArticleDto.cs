using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RallyKnowledgeOwlIntegration
{
    class ArticleDto
    {
        //knowledge owl crossroads/ministry platform project id
        [JsonProperty(PropertyName = "project_id")]
        public string ProjectId { get; set; }

        //article title
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        //html for the body of the article
        [JsonProperty(PropertyName = "current_version")]
        public string CurrentVersion { get; set; }

        //category folder of the article
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        //"ready", "rejected", "published", "review", "deleted"
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }        
    }
}
