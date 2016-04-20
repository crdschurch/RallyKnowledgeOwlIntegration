using System.Linq;

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
