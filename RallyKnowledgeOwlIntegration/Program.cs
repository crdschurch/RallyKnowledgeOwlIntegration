using System;
using log4net;
using RallyKnowledgeOwlIntegration.Helpers;
using RallyKnowledgeOwlIntegration.Services;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace RallyKnowledgeOwlIntegration
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                _logger.Info("Starting process");
                AutoMapperConfig.RegisterMappings();

                var rally = new RallyDataService();
                var artifacts = rally.LoadArtifactsByState();

                var knowledgeOwl = new KnowledgeOwlDataService();
                knowledgeOwl.UpdateBacklogArticle(artifacts);

                _logger.Info("Finished process successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Error running process due to exception", ex);
                throw;
            }
        }
    }
}