using HelpDev.Infrastructure.Ai;
using HelpDev.Modules.Content;
using HelpDev.Modules.Learning;
using HelpDev.Modules.PromptLab;
using HelpDev.Modules.Search;
using HelpDev.Modules.Search.Application.Chunking;
using HelpDev.Modules.Search.Application.Rag;
using HelpDev.Modules.Search.Application.Semantic;
using HelpDev.Modules.Toolbox;
using HelpDev.SharedContracts.Ai;
using NetArchTest.Rules;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using LearningModuleMarker = HelpDev.Modules.Learning.ModuleMarker;
using PromptLabModuleMarker = HelpDev.Modules.PromptLab.ModuleMarker;
using SearchModuleMarker = HelpDev.Modules.Search.ModuleMarker;
using ToolboxModuleMarker = HelpDev.Modules.Toolbox.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class SemanticSearchArchitectureTests
{
    [Fact]
    public void Source_modules_do_not_depend_on_Search_or_pgvector()
    {
        foreach (var assembly in new[]
                 {
                     typeof(ContentModuleMarker).Assembly,
                     typeof(LearningModuleMarker).Assembly,
                     typeof(ToolboxModuleMarker).Assembly,
                     typeof(PromptLabModuleMarker).Assembly,
                 })
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(
                    "HelpDev.Modules.Search",
                    "HelpDev.Infrastructure.Search",
                    "Pgvector")
                .GetResult();

            Assert.True(result.IsSuccessful, FormatFailures(result));
        }
    }

    [Fact]
    public void Search_Application_does_not_depend_on_AI_provider_infrastructure()
    {
        var result = Types.InAssembly(typeof(SearchModuleMarker).Assembly)
            .That()
            .ResideInNamespaceStartingWith("HelpDev.Modules.Search.Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Infrastructure.Ai",
                "OpenAI",
                "Anthropic")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Controllers_do_not_take_vector_or_DbContext_dependencies()
    {
        foreach (var controller in new[]
                 {
                     typeof(HelpDev.API.Controllers.SearchController),
                     typeof(HelpDev.API.Controllers.SearchManageController),
                 })
        {
            var ctor = controller.GetConstructors().Single();
            Assert.DoesNotContain(
                ctor.GetParameters(),
                p => p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal)
                     || p.ParameterType.Name.Contains("Vector", StringComparison.Ordinal)
                     || p.ParameterType.Namespace?.Contains("Pgvector", StringComparison.Ordinal) == true);
        }
    }

    [Fact]
    public void Dtos_never_expose_embeddings()
    {
        foreach (var dto in new[]
                 {
                     typeof(SearchContextDto),
                     typeof(SearchContextItemDto),
                     typeof(SemanticSearchResultDto),
                     typeof(SemanticSearchResponseDto),
                     typeof(RagAnswerDto),
                     typeof(RagSourceDto),
                     typeof(RagContext),
                     typeof(ContentChunkDto),
                 })
        {
            var names = dto.GetProperties().Select(p => p.Name).ToArray();
            Assert.DoesNotContain(names, n => n.Contains("Embedding", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(names, n => n.Contains("Vector", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(names, n => n.Contains("ApiKey", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Embedding_abstraction_is_shared_and_fake_lives_in_Infrastructure()
    {
        Assert.True(typeof(IEmbeddingGenerator).IsAssignableFrom(typeof(FakeEmbeddingGenerator)));
        Assert.StartsWith("HelpDev.Infrastructure.Ai", typeof(FakeEmbeddingGenerator).Namespace);
        Assert.StartsWith("HelpDev.SharedContracts.Ai", typeof(IEmbeddingGenerator).Namespace);
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
