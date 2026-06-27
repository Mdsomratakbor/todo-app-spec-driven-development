using Cartographer.Core.Abstractions;
using Cartographer.Core.Configuration;

namespace TodoApi.Models.Mappings;

public class MappingProfile : Profile
{
    protected override void ConfigureMappings(IMapperConfigurationExpression cfg)
    {
        // Entity-to-DTO mappings will be added when entities and DTOs are created.
        // See Tasks 4.1 (Category DTOs) and 5.1 (Todo DTOs).
    }
}
