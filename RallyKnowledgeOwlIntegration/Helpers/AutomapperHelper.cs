using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace RallyKnowledgeOwlIntegration.Helpers
{
    class AutoMapperHelper
    {
        public static List<T> MapToListObjects<T>(IEnumerable<dynamic> results)
        {
            var iterations = new List<T>();

            var config = new MapperConfiguration(cfg => { });
            var mapper = config.CreateMapper();

            foreach (var result in results)
            {
                var iteration = mapper.Map<T>(result);
                iterations.Add(iteration);
            }
            return iterations;
        }
    }
}
