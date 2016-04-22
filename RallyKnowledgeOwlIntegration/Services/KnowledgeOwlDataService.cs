using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using RallyKnowledgeOwlIntegration.Models;
using RestSharp;
using RestSharp.Authenticators;

namespace RallyKnowledgeOwlIntegration.Services
{
    class KnowledgeOwlDataService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(KnowledgeOwlDataService));

        public void UpdateBacklogArticle(RallyArtifactsByState artifactsByState)
        {            
            string apiKey = Environment.GetEnvironmentVariable("KNOWLEDGE_OWL_API_KEY");
            string articleId = Environment.GetEnvironmentVariable("KNOWLEDGE_OWL_ARTICLE_ID");
            var knowledgeOwlRestClient = new RestClient("https://app.knowledgeowl.com/api/head/");
            knowledgeOwlRestClient.Authenticator = new HttpBasicAuthenticator(apiKey, "AnyFooBarPassword");
            if (knowledgeOwlRestClient == null) throw new ArgumentNullException(nameof(knowledgeOwlRestClient));

            var backlogTable = CreateTable(artifactsByState.Backlog);
            var currentTable = CreateTable(artifactsByState.CurrentIteration);
            var previousTable = CreateTable(artifactsByState.PreviousIterations);

            var urlPut = string.Format("article/{0}.json", articleId);
            var requestPut = new RestRequest(urlPut, Method.PUT);
            requestPut.Parameters.Clear();
            requestPut.RequestFormat = DataFormat.Json;
            requestPut.AddHeader("accept", "application/json, text/plain, */*");
            requestPut.AddHeader("content-type", "application/json");

            var article = new Article();
            article.current_version.en.text = CreateBody(previousTable, currentTable, backlogTable);
            requestPut.AddJsonBody(article); //serializes the object automatically
            var responsePut = knowledgeOwlRestClient.Execute(requestPut);
            _logger.Debug(responsePut.Content);
        }

        private static string CreateTable(List<RallyArtifact> artifacts)
        {
            StringBuilder sbHtmlTable = new StringBuilder();
            sbHtmlTable.Append("<table align = 'center' border = '1' bordercolor = '#ccc' cellpadding = '5' cellspacing = '0' class='table table-small-font table-condensed table-bordered table-responsive' style='border-collapse:collapse;'><thead><tr><th scope = 'col'>ID</th><th scope='col'>Name&nbsp;</th><th scope = 'col' >Status&nbsp;</th><th scope = 'col'> Iteration</th><th scope='col'>Target Deployment Date</th></tr></thead><tbody>");
            StringBuilder sbRallyContent = new StringBuilder();
            foreach (var item in artifacts)
            {
                var targetDate = item.TargetDate.HasValue ? item.TargetDate.Value.ToShortDateString() : string.Empty;

                sbRallyContent.Append("<tr><td>");
                sbRallyContent.Append(item.FormattedId);
                sbRallyContent.Append("</td><td>");
                sbRallyContent.Append(item.Name);
                sbRallyContent.Append("</td><td>");
                sbRallyContent.Append(item.Status);
                sbRallyContent.Append("</td><td>");
                sbRallyContent.Append(item.IterationName);
                sbRallyContent.Append("</td><td>");
                sbRallyContent.Append(targetDate);
            }
            sbHtmlTable.Append(sbRallyContent.ToString().Replace("\"", "'"));
            sbHtmlTable.Append("</tbody></table>");
            return sbHtmlTable.ToString();
        }

        private static string CreateBody(string tablePreviousSprint, string tableCurrentSprint, string tableBacklog)
        {
            StringBuilder sbBodyHtml = new StringBuilder();
            sbBodyHtml.Append("<p><a href='%5B%5Bhg-id:57177b6632131cd43b6a2394%5D%5D' title='IT Prioritization'>IT Prioritization</a>&nbsp;</p><p>(Placehoder text from OCM to describe the article.)</p><p></p>");
            sbBodyHtml.Append(tablePreviousSprint);
            sbBodyHtml.Append("<p>(Placehoder text from OCM to describe the article.)</p>");
            sbBodyHtml.Append(tableCurrentSprint);
            sbBodyHtml.Append("<p>(Placehoder text from OCM to describe the article.)</p>");
            sbBodyHtml.Append(tableBacklog);
            return sbBodyHtml.ToString();
        }
    }
}
