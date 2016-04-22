using System.Collections.Generic;
using AutoMapper;

namespace RallyKnowledgeOwlIntegration.Helpers
{
    class AutoMapperHelper
    {
        public static List<T> MapToListObjects<T>(IEnumerable<dynamic> results)
        {
            var items = new List<T>();

            foreach (var result in results)
            {
                var item = Mapper.Map<T>(result.ToDictionary());
                items.Add(item);
            }

            return items;
        }
    }
}
