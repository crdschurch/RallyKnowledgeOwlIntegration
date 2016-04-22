using Newtonsoft.Json;
using RallyKnowledgeOwlIntegration.Helpers;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace RallyKnowledgeOwlIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            AutoMapperConfig.RegisterMappings();

            var rally = new RallyDataService();
            var artifacts = rally.LoadArtifactsByState();            

            var knowledgeOwl = new KnowledgeOwlDataService();
            knowledgeOwl.UpdateBacklogArticle(artifacts);
        }
    }
}