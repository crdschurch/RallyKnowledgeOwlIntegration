using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using RestSharp.Authenticators;
using log4net;
using RallyKnowledgeOwlIntegration.Models;

namespace RallyKnowledgeOwlIntegration
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

            StringBuilder sbBodyHtml = new StringBuilder();            
            sbBodyHtml.Append("{\"current_version\": {\"en\": {\"text\":\"<p><a href='%5B%5Bhg-id:57177b6632131cd43b6a2394%5D%5D' title='IT Prioritization'>IT Prioritization</a>&nbsp;</p><p>(Placehoder text from OCM to describe the article.)</p><p></p>");
            //sbBodyHtml.Append("{\"current_version\": {\"en\": {\"text\":\"<p><a href='%5B%5Bhg-id:57177b6632131cd43b6a2394%5D%5D' title='IT Prioritization'>IT Prioritization</a>&nbsp;</p><p>(Placehoder text from OCM to describe the article.)</p><p></p>");
            sbBodyHtml.Append(currentTable);
            sbBodyHtml.Append("<p>(Placehoder text from OCM to describe the article.)</p>");
            sbBodyHtml.Append(previousTable);
            sbBodyHtml.Append("<p>(Placehoder text from OCM to describe the article.)</p>");
            sbBodyHtml.Append(backlogTable);
            sbBodyHtml.Append("\"}}}");            

            var urlPut = string.Format("article/{0}.json", articleId);
            var requestPut = new RestRequest(urlPut, Method.PUT);
            requestPut.Parameters.Clear();
            requestPut.RequestFormat = DataFormat.Json;
            requestPut.AddHeader("accept", "application/json, text/plain, */*");
            requestPut.AddHeader("content-type", "application/json");
            // requestPut.AddParameter("application/json", sbBodyHtml.ToString(), ParameterType.RequestBody);
            //requestPut.AddJsonBody(new {content = sbBodyHtml.ToString()}); //serializes the object automatically

            var article = new ArticleDto();
            article.CurrentVersion.Language.Text = sbBodyHtml.ToString();
            requestPut.AddJsonBody(article); //serializes the object automatically

            var responsePut = knowledgeOwlRestClient.Execute(requestPut);
            _logger.Debug(responsePut.StatusCode);
            _logger.Debug(responsePut.Content);
        }

        private static string CreateTable(List<RallyArtifact> artifacts)
        {
            StringBuilder sbHtmlTable = new StringBuilder();
            sbHtmlTable.Append("<table align = 'center' border = '1' bordercolor = '#ccc' cellpadding = '5' cellspacing = '0' class='table table-small-font table-condensed table-bordered table-responsive' style='border-collapse:collapse;'><thead><tr><th scope = 'col'>ID</th><th scope='col'>Name&nbsp;</th><th scope = 'col' >Status&nbsp;</th><th scope = 'col'> Iteration</th><th scope='col'>Target Deployment Date</th></tr></thead><tbody>");
            StringBuilder sbRallyContent = new StringBuilder();
            foreach (var item in artifacts)
            {
<<<<<<< HEAD
                sbRallyContent.Append("<tr><td>");
                sbRallyContent.Append(item.FormattedId);
                sbRallyContent.Append("</td><td>");
                sbRallyContent.Append(item.Name);
                sbRallyContent.Append("</td><td>");
                sbRallyContent.Append(item.Status);
                sbRallyContent.Append("</td><td>");
                sbRallyContent.Append(item.IterationName);
                sbRallyContent.Append("</td><td>?Date?</td></tr>");
=======
                var targetDate = item.TargetDate.HasValue ? item.TargetDate.Value.ToShortDateString() : string.Empty;
                table += "<tr><td>" + item.FormattedId + "</td><td>" + item.Name + "</td><td>" + item.Status + "</td><td>" +
                         item.IterationName + "</td><td>" + targetDate + "</td></tr>";
>>>>>>> 124d4b8f444341aa4f268ec5fc7c8db0fc8540c4
            }
            //sbHtmlTable.Append(sbRallyContent.ToString().Replace("\"", "'"));
            sbHtmlTable.Append("</tbody></table>");
            return sbHtmlTable.ToString();
        }
    }
}
