using System.Collections.Generic;
using AutoMapper;
using RestSharp.Extensions;

namespace RallyKnowledgeOwlIntegration.Models
{
    public class RallyArtifactProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Dictionary<string, object>, RallyArtifact>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src["Name"]))
                .ForMember(dest => dest.FormattedId, opts => opts.MapFrom(src => src["FormattedID"]))
                .ForMember(dest => dest.ScheduleState, opts => opts.MapFrom(src => src["ScheduleState"]))
                .ForMember(dest => dest.KanbanState, opts => opts.MapFrom(src => src["c_CrossroadsKanbanState"]))
                .ForMember(dest => dest.Priority,
                    opts => opts.MapFrom(src => src.ContainsKey("Priority") ? src["Priority"] : null))
                .ForMember(dest => dest.IterationName,
                    opts =>
                        opts.MapFrom(
                            src =>
                                ((Dictionary<string, object>) src["Iteration"])
                                    .ContainsKey("Name")
                                    ? ((Dictionary<string, object>)src["Iteration"])["Name"]
                                    : string.Empty
                            ));



        }
    }
}