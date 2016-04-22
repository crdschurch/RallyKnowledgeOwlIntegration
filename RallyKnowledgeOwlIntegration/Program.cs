using System;
using log4net;
using RallyKnowledgeOwlIntegration.Helpers;
using RallyKnowledgeOwlIntegration.Services;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace RallyKnowledgeOwlIntegration
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                Logger.Info("Starting process");
                AutoMapperConfig.RegisterMappings();

                var rally = new RallyDataService();
                var artifacts = rally.LoadArtifactsByState();

                var knowledgeOwl = new KnowledgeOwlDataService();
                knowledgeOwl.UpdateBacklogArticle(artifacts);

                Logger.Info("Finished process successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error running process due to exception", ex);
                throw;
            }
        }
    }
}