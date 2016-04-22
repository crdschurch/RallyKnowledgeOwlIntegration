using System.Collections.Generic;

namespace RallyKnowledgeOwlIntegration.Models
{
    public class RallyIterationProfile : AutoMapper.Profile
    {
        protected override void Configure()
        {
            AutoMapper.Mapper.CreateMap<Dictionary<string, object>, RallyIteration>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src["Name"]))
                .ForMember(dest => dest.StartDate, opts => opts.MapFrom(src => src["StartDate"]))
                .ForMember(dest => dest.EndDate, opts => opts.MapFrom(src => src["EndDate"]));
        }
    }
}