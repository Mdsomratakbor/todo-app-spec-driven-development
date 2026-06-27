using TodoApi.Models.Mappings;

namespace TodoApi.Tests.Mappings;

public class MappingProfileTests
{
    [Fact]
    public void MappingProfile_Can_Create_Instance()
    {
        var profile = new MappingProfile();
        Assert.NotNull(profile);
    }

    [Fact]
    public void MappingProfile_IsProfileType()
    {
        var profile = new MappingProfile();
        Assert.IsType<MappingProfile>(profile);
    }
}
