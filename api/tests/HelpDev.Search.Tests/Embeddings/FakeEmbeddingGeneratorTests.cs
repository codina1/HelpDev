using HelpDev.Infrastructure.Ai;
using HelpDev.Modules.Search.Infrastructure.Persistence;
using HelpDev.SharedContracts.Ai;
using Microsoft.Extensions.Options;

namespace HelpDev.Search.Tests.Embeddings;

public sealed class FakeEmbeddingGeneratorTests
{
    [Fact]
    public async Task Generate_returns_configured_dimensions_without_logging_text()
    {
        var generator = new FakeEmbeddingGenerator(Options.Create(new EmbeddingOptions
        {
            Enabled = true,
            ProviderName = "Fake",
            Model = "fake-embed-v1",
            Dimensions = SearchVectorConfiguration.DefaultDimensions,
        }));

        var a = await generator.GenerateAsync("postgresql vector search");
        var b = await generator.GenerateAsync("postgresql vector search");
        var c = await generator.GenerateAsync("completely unrelated cooking recipe");

        Assert.Equal(SearchVectorConfiguration.DefaultDimensions, a.Dimensions);
        Assert.Equal(a.Vector, b.Vector);
        Assert.Equal("Fake", a.Provider);
        Assert.NotEqual(a.Vector, c.Vector);
    }

    [Fact]
    public void Contract_is_provider_agnostic()
    {
        Assert.Equal(typeof(IEmbeddingGenerator), typeof(FakeEmbeddingGenerator).GetInterfaces().Single(i => i == typeof(IEmbeddingGenerator)));
        Assert.DoesNotContain(
            typeof(EmbeddingResult).GetProperties().Select(p => p.Name),
            name => name.Contains("ApiKey", StringComparison.OrdinalIgnoreCase));
    }
}
