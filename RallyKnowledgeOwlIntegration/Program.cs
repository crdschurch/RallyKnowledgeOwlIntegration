using System;
using System.Collections.Generic;

namespace RallyKnowledgeOwlIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            var rally = new RallyDataService();
            var result = rally.LoadStoriesAndDefects();

            var knowledgeOwl = new KnowledgeOwlDataService();
            knowledgeOwl.UpdateBacklogArticle(result);
        }
    }
}
