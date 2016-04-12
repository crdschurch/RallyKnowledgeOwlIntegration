using System;
using System.Collections.Generic;
using Rally.RestApi;
using Rally.RestApi.Response;

namespace RallyKnowledgeOwlIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            var rally = new RallyDataService();
            var result = rally.LoadStoriesAndDefects();

        }
    }
}
