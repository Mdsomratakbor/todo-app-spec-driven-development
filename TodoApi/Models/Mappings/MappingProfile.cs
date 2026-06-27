using Cartographer.Core.Abstractions;
using Cartographer.Core.Configuration;
using TodoApi.Models.DTOs.Categories;
using TodoApi.Models.DTOs.Todos;
using TodoApi.Models.Entities;

namespace TodoApi.Models.Mappings;

public class MappingProfile : Profile
{
    protected override void ConfigureMappings(IMapperConfigurationExpression cfg)
    {
        cfg.CreateMap<Category, CategoryResponse>()
            .ForMember(d => d.TodoCount, o => o.Ignore());

        cfg.CreateMap<Todo, TodoResponse>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null));
    }
}
