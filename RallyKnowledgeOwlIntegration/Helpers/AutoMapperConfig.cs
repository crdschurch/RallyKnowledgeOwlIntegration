using AutoMapper;
using RallyKnowledgeOwlIntegration.Models;

namespace RallyKnowledgeOwlIntegration.Helpers
{
    public static class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<RallyArtifactProfile>();
                cfg.AddProfile<RallyIterationProfile>();
            });
        }
    }
}