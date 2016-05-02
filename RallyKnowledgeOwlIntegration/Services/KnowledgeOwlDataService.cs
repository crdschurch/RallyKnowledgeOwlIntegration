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
            string apiKey = Environment.GetEnvironmentVariable("KNOWLEDGE_OWL_API_KEY").Trim();
            string articleId = Environment.GetEnvironmentVariable("KNOWLEDGE_OWL_ARTICLE_ID").Trim();
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
            sbBodyHtml.Append("<p>Below is the backlog of issues (we call them defects) and some minor enhancements from all staff including some items we've discovered or want to resolve.  If you submitted a help desk ticket and were  told development was required and it would be prioritized, this is how you see those items.  </p><p>You can search the page for your help desk ticket number or the name of the request you submitted.This is not our feature backlog, which tracks improvements and new features like Text to Give or Replacing eCheck.To see that backlog with projected timeline, click <a href='%5B%5Bhg-id:5718eca832131c6e5d6a22c4%5D%5D' style='line-height: 1.42857;' target='_blank' title='IT Feature Projections'> here </a><span style ='line-height: 1.42857;' >.  </span></p><p></p><p>If you have questions about how IT thinks about priority, first check the <a href='%5B%5Bhg-id:57177b6632131cd43b6a2394%5D%5D' target='_blank' title='IT Prioritization'> IT prioritization page</a>.  If you still have a question on priority of a particular item then we are glad to hear from you.  We'd like to understand the impact and value of the item for which  you are advocating. To communicate this to us, please send an email to <a href='mailto:helpdesk@crossroads.net'>helpdesk@crossroads.net</a>.</p>");
            sbBodyHtml.Append("<h3>Completed &amp; Deployed Items</h3><p>These features and fixes are completed and in the system now.These items were released in our most recent deployment on the date listed below. </p><p> If you have any issues with these, please reach out to <a href='mailto:helpdesk@crossroads.net.'>helpdesk@crossroads.net </a>.If you need training on any of these items, please attend one of our upcoming MP Open Hour<span style='line-height: 1.42857;'>s.Click <a href='https://calendar.google.com/calendar/embed?src=Y3Jvc3Nyb2Fkcy5uZXRfODZpa2Rrcmt2MHVpNjIwZGQxaWJrc2w0MnNAZ3JvdXAuY2FsZW5kYXIuZ29vZ2xlLmNvbQ'> here </a> to see calendar  </span></p><p></p><p></p>");
            sbBodyHtml.Append(tablePreviousSprint);
            sbBodyHtml.Append("<h3>Items In Progress</h3><p> These items are actively  being  worked or are planned to be worked in our current sprint.We work in 2 week iterations and put new features and fixes into your world at the end  of that period.  </p><p> If the item on the list says it is on the backlog and does not have a date then know we plan to get to this but haven't yet committed, it will depend on what other priority items come up during the sprint.</p><p> If you have any concerns with these priorities please reach out to <a href='mailto:helpdesk@crossroads.net'> helpdesk@crossroads.net </a>.If you anticipate needing training on any of these items  please attend one of our upcoming <a href='.https://calendar.google.com/calendar/embed?src=Y3Jvc3Nyb2Fkcy5uZXRfODZpa2Rrcmt2MHVpNjIwZGQxaWJrc2w0MnNAZ3JvdXAuY2FsZW5kYXIuZ29vZ2xlLmNvbQ'> MP Open Hours</a>  (if someone from IT is not already working with you on the item).</p> ");
            sbBodyHtml.Append(tableCurrentSprint);
            sbBodyHtml.Append("<h3>Backlog Items  </h3><p> These items are on our backlog, in priority order.  They came from help tickets you (staff) entered, and items we've found that need resolved or improved. If you have any concerns or question on priority please reach out to <a href='mailto:helpdesk@crossroads.net'>helpdesk@crossroads.net</a>. </p><p> If you anticipate needing training on any of these items please attend one of our upcoming <a href='https://calendar.google.com/calendar/embed?src=Y3Jvc3Nyb2Fkcy5uZXRfODZpa2Rrcmt2MHVpNjIwZGQxaWJrc2w0MnNAZ3JvdXAuY2FsZW5kYXIuZ29vZ2xlLmNvbQ'> MP Open Hours</a> (if someone from IT is not already working with you on the item).</p> ");
            sbBodyHtml.Append(tableBacklog);
            return sbBodyHtml.ToString();
        }
    }
}
