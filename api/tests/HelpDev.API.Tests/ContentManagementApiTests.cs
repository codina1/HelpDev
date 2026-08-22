using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Content.Application.Articles;
using HelpDev.Modules.Content.Application.Articles.Dtos;
using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Application.News;
using HelpDev.Modules.Content.Application.News.Dtos;
using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Application.Roadmaps;
using HelpDev.Modules.Content.Application.Roadmaps.Ai;
using HelpDev.Modules.Content.Application.Roadmaps.Dtos;
using HelpDev.Modules.Content.Application.Tools;
using HelpDev.Modules.Content.Application.Tools.Ai;
using HelpDev.Modules.Content.Application.Tools.Dtos;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace HelpDev.API.Tests;

public sealed class ContentManagementApiTests
{
    [Fact]
    public void Management_controller_requires_writer_or_admin_policy()
    {
        var attribute = Assert.Single(
            typeof(ContentManagementController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.WriterOrAdmin, attribute.Policy);
    }

    [Fact]
    public async Task List_scopes_writer_to_own_content()
    {
        var queries = new FakeAdminContentQueries();
        var controller = CreateManagementController(new FakeContentService(), queries);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);

        await controller.List(search: null, status: null, type: null, page: null, pageSize: null, CancellationToken.None);

        Assert.NotNull(queries.LastFilter);
        Assert.Equal(userId, queries.LastFilter!.AuthorId);
    }

    [Fact]
    public async Task List_lets_admin_see_all_content()
    {
        var queries = new FakeAdminContentQueries();
        var controller = CreateManagementController(new FakeContentService(), queries);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        await controller.List(search: null, status: null, type: null, page: null, pageSize: null, CancellationToken.None);

        Assert.Null(queries.LastFilter!.AuthorId);
    }

    [Fact]
    public async Task List_forwards_and_clamps_paging_and_filters()
    {
        var queries = new FakeAdminContentQueries();
        var controller = CreateManagementController(new FakeContentService(), queries);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        var result = await controller.List(
            search: "intro",
            status: "Draft",
            type: "Article",
            page: 2,
            pageSize: 5000,
            CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        var filter = queries.LastFilter!;
        Assert.Equal("intro", filter.Search);
        Assert.Equal("Draft", filter.Status);
        Assert.Equal("Article", filter.Type);
        Assert.Equal(2, filter.Page);
        Assert.Equal(ContentSearchFilter.MaxPageSize, filter.PageSize);
    }

    [Fact]
    public async Task List_returns_unauthorized_when_user_missing()
    {
        var controller = CreateManagementController(new FakeContentService(), new FakeAdminContentQueries());
        ControllerTestHelper.SetUser(controller, userId: null);

        var result = await controller.List(null, null, null, null, null, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetById_forwards_route_id_and_actor()
    {
        var service = new FakeContentService();
        var controller = CreateManagementController(service, new FakeAdminContentQueries());
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var contentId = Guid.NewGuid();

        var result = await controller.GetById(contentId, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(contentId, service.LastContentId);
        Assert.Equal(userId, service.LastActor!.UserId);
        Assert.False(service.LastActor.CanManageAllContent);
        Assert.Equal(nameof(IContentService.GetManagedByIdAsync), service.LastOperation);
    }

    [Fact]
    public async Task GetById_admin_actor_can_manage_all()
    {
        var service = new FakeContentService();
        var controller = CreateManagementController(service, new FakeAdminContentQueries());
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.True(service.LastActor!.CanManageAllContent);
    }

    [Fact]
    public async Task GetById_returns_unauthorized_when_user_missing()
    {
        var controller = CreateManagementController(new FakeContentService(), new FakeAdminContentQueries());
        ControllerTestHelper.SetUser(controller, userId: null);

        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public void Admin_detail_dto_contract_is_stable()
    {
        var props = typeof(AdminContentDetailDto).GetProperties().Select(p => p.Name).ToArray();

        Assert.Contains("Id", props);
        Assert.Contains("Title", props);
        Assert.Contains("Slug", props);
        Assert.Contains("Body", props);
        Assert.Contains("Excerpt", props);
        Assert.Contains("CoverImage", props);
        Assert.Contains("ContentType", props);
        Assert.Contains("ContentStatus", props);
        Assert.Contains("AuthorId", props);
        Assert.Contains("CreatedAtUtc", props);
        Assert.Contains("UpdatedAtUtc", props);
        Assert.Contains("PublishedAtUtc", props);
        Assert.Contains("Seo", props);

        // Must not leak Domain/EF types.
        Assert.Null(typeof(AdminContentDetailDto).GetProperty("SeoMetadata"));
        Assert.DoesNotContain(typeof(AdminContentDetailDto).GetProperties(), p =>
            p.PropertyType.Namespace?.StartsWith("HelpDev.Modules.Content.Domain", StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task Update_forwards_route_id_actor_and_request()
    {
        var service = new FakeContentService();
        var controller = CreateManagementController(service, new FakeAdminContentQueries());
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var contentId = Guid.NewGuid();

        var request = new UpdateContentRequest
        {
            Title = "T",
            Slug = "t",
            Type = "Article",
            Body = "B",
        };

        var result = await controller.Update(contentId, request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(contentId, service.LastContentId);
        Assert.Equal(userId, service.LastActor!.UserId);
        Assert.False(service.LastActor.CanManageAllContent);
        Assert.Equal(nameof(IContentService.UpdateAsync), service.LastOperation);
        Assert.Same(request, service.LastUpdateRequest);
    }

    [Fact]
    public void Preview_compiles_article_json_without_persisting()
    {
        var service = new FakeContentService();
        var controller = CreateManagementController(service, new FakeAdminContentQueries());
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Writer);

        var result = controller.PreviewArticle(new PreviewArticleRequest("""{"type":"doc","content":[]}""", "body"));

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(nameof(IContentService.Preview), service.LastOperation);
        Assert.Null(service.LastContentId);
    }

    [Fact]
    public async Task Publish_forwards_route_id_and_admin_actor()
    {
        var workflow = new FakeContentWorkflowService();
        var controller = CreateManagementController(new FakeContentService(), workflowService: workflow);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Admin);
        var contentId = Guid.NewGuid();

        var result = await controller.Publish(contentId, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(contentId, workflow.LastContentId);
        Assert.True(workflow.LastActor!.CanManageAllContent);
        Assert.Equal(nameof(IContentWorkflowService.PublishAsync), workflow.LastOperation);
    }

    [Fact]
    public async Task Publish_returns_unauthorized_when_user_missing()
    {
        var controller = CreateManagementController(new FakeContentService(), new FakeAdminContentQueries());
        ControllerTestHelper.SetUser(controller, userId: null);

        var result = await controller.Publish(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Malformed_user_id_returns_unauthorized()
    {
        var controller = CreateManagementController(new FakeContentService(), new FakeAdminContentQueries());
        ControllerTestHelper.SetMalformedUserId(controller, "not-a-guid");

        var result = await controller.List(null, null, null, null, null, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public void Controller_depends_on_application_interfaces_not_repositories()
    {
        var ctor = typeof(ContentManagementController).GetConstructors().Single();

        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IContentService));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IAdminContentQueries));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IContentRevisionQueries));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IContentRevisionService));
        Assert.DoesNotContain(
            ctor.GetParameters(),
            p => p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                 || p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public void Update_request_does_not_expose_ownership_flags()
    {
        Assert.Null(typeof(UpdateContentRequest).GetProperty("AuthorId"));
        Assert.Null(typeof(UpdateContentRequest).GetProperty("CanManageAllContent"));
    }

    [Fact]
    public async Task UpdateSeo_forwards_route_id_actor_and_request()
    {
        var service = new FakeContentService();
        var controller = CreateManagementController(service, new FakeAdminContentQueries());
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var contentId = Guid.NewGuid();

        var request = new UpdateSeoMetadataRequest
        {
            SeoTitle = "Title",
            SeoDescription = "Description",
            CanonicalUrl = "https://helpdev.example/a",
        };

        var result = await controller.UpdateSeo(contentId, request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(contentId, service.LastContentId);
        Assert.Equal(userId, service.LastActor!.UserId);
        Assert.False(service.LastActor.CanManageAllContent);
        Assert.Equal(nameof(IContentService.UpdateSeoMetadataAsync), service.LastOperation);
        Assert.Same(request, service.LastSeoRequest);
    }

    [Fact]
    public async Task UpdateSeo_returns_unauthorized_when_user_missing()
    {
        var controller = CreateManagementController(new FakeContentService(), new FakeAdminContentQueries());
        ControllerTestHelper.SetUser(controller, userId: null);

        var result = await controller.UpdateSeo(
            Guid.NewGuid(),
            new UpdateSeoMetadataRequest(),
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public void Admin_detail_dto_exposes_seo_but_public_detail_does_not()
    {
        Assert.NotNull(typeof(AdminContentDetailDto).GetProperty("Seo"));
        Assert.Equal(typeof(SeoMetadataDto), typeof(AdminContentDetailDto).GetProperty("Seo")!.PropertyType);

        var publicProps = typeof(ContentDetailDto).GetProperties().Select(p => p.Name).ToArray();
        Assert.DoesNotContain("Seo", publicProps);
        Assert.DoesNotContain("SeoTitle", publicProps);
        Assert.DoesNotContain("CanonicalUrl", publicProps);
        Assert.DoesNotContain("FocusKeyword", publicProps);
    }

    [Fact]
    public void Seo_metadata_dto_contract_is_stable()
    {
        var props = typeof(SeoMetadataDto).GetProperties().Select(p => p.Name).ToArray();

        Assert.Equal(
            new[] { "SeoTitle", "SeoDescription", "CanonicalUrl", "OgImage", "FocusKeyword" },
            props);
    }

    [Fact]
    public async Task AnalyzeSeo_forwards_route_id_and_actor()
    {
        var service = new FakeContentService();
        var controller = CreateManagementController(service, new FakeAdminContentQueries());
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var contentId = Guid.NewGuid();

        var result = await controller.AnalyzeSeo(contentId, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(contentId, service.LastContentId);
        Assert.Equal(userId, service.LastActor!.UserId);
        Assert.False(service.LastActor.CanManageAllContent);
        Assert.Equal(nameof(IContentService.AnalyzeSeoAsync), service.LastOperation);
    }

    [Fact]
    public async Task AnalyzeSeo_admin_actor_can_manage_all()
    {
        var service = new FakeContentService();
        var controller = CreateManagementController(service, new FakeAdminContentQueries());
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        await controller.AnalyzeSeo(Guid.NewGuid(), CancellationToken.None);

        Assert.True(service.LastActor!.CanManageAllContent);
    }

    [Fact]
    public async Task AnalyzeSeo_returns_unauthorized_when_user_missing()
    {
        var controller = CreateManagementController(new FakeContentService(), new FakeAdminContentQueries());
        ControllerTestHelper.SetUser(controller, userId: null);

        var result = await controller.AnalyzeSeo(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public void AnalyzeSeo_endpoint_is_post_seo_analysis_route()
    {
        var method = typeof(ContentManagementController).GetMethod(nameof(ContentManagementController.AnalyzeSeo));
        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttributes(typeof(HttpPostAttribute), inherit: false).Single());
        var template = Assert.IsType<HttpPostAttribute>(
            method.GetCustomAttributes(typeof(HttpPostAttribute), inherit: false).Single()).Template;
        Assert.Equal("{id:guid}/seo-analysis", template);
    }

    [Fact]
    public void Seo_analysis_report_dto_contract_has_no_score_or_domain_leakage()
    {
        var props = typeof(SeoAuditReportDto).GetProperties().Select(p => p.Name).ToArray();
        Assert.Contains("ContentId", props);
        Assert.Contains("GeneratedAtUtc", props);
        Assert.Contains("Summary", props);
        Assert.Contains("Findings", props);
        Assert.DoesNotContain(props, p => p.Contains("Score", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Percent", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Rank", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Statistics", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(typeof(SeoAuditReportDto).GetProperties(), p =>
            p.PropertyType.Namespace?.StartsWith("HelpDev.Modules.Content.Domain", StringComparison.Ordinal) == true
            || p.PropertyType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Seo_platform_category_enum_is_stable()
    {
        Assert.Equal(new[] { "Info", "Warning", "Error" }, Enum.GetNames<SeoFindingSeverity>());
        Assert.Equal(
            new[] { "Metadata", "ContentStructure", "Images", "Links", "Technical" },
            Enum.GetNames<SeoPlatformCategory>());
    }

    [Fact]
    public async Task ListRevisions_forwards_actor_and_paging()
    {
        var revisionQueries = new FakeContentRevisionQueries();
        var controller = CreateManagementController(new FakeContentService(), revisionQueries: revisionQueries);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var contentId = Guid.NewGuid();

        await controller.ListRevisions(contentId, page: 2, pageSize: 10, CancellationToken.None);

        Assert.Equal(contentId, revisionQueries.LastContentId);
        Assert.Equal(userId, revisionQueries.LastActor!.UserId);
    }

    [Fact]
    public async Task RestoreRevision_delegates_to_revision_service()
    {
        var revisionService = new FakeContentRevisionService();
        var controller = CreateManagementController(new FakeContentService(), revisionService: revisionService);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);
        var contentId = Guid.NewGuid();

        await controller.RestoreRevision(contentId, revisionVersion: 2, new RestoreContentRevisionRequest("undo"), CancellationToken.None);

        Assert.Equal(contentId, revisionService.LastContentId);
        Assert.Equal(2, revisionService.LastVersion);
    }

    [Fact]
    public async Task Ai_analyze_returns_result_without_sensitive_fields()
    {
        var ai = new FakeContentAiAssistantService();
        var controller = CreateManagementController(new FakeContentService(), contentAiAssistant: ai);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var contentId = Guid.NewGuid();

        var result = await controller.AiAnalyze(contentId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ContentAiResultDto>(ok.Value);
        Assert.Equal("ContentAnalysis", dto.TaskType);
        Assert.Equal("sample generated text", dto.GeneratedText);
        Assert.Equal("fake-v1", dto.Model);
        Assert.Equal("Fake", dto.Provider);
        Assert.Equal(nameof(FakeContentAiAssistantService.AnalyzeContentAsync), ai.LastOperation);
        Assert.Equal(contentId, ai.LastContentId);

        var names = typeof(ContentAiResultDto).GetProperties().Select(p => p.Name).ToArray();
        Assert.DoesNotContain(names, n => n.Contains("ApiKey", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Prompt", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("SystemInstruction", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Secret", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(nameof(ContentManagementController.AiTitleSuggestions), "GenerateTitleSuggestionsAsync")]
    [InlineData(nameof(ContentManagementController.AiMetaDescription), "GenerateMetaDescriptionAsync")]
    [InlineData(nameof(ContentManagementController.AiOutline), "GenerateOutlineAsync")]
    [InlineData(nameof(ContentManagementController.AiFaq), "GenerateFaqAsync")]
    public async Task Ai_endpoints_delegate_to_assistant(string actionName, string expectedOperation)
    {
        var ai = new FakeContentAiAssistantService();
        var controller = CreateManagementController(new FakeContentService(), contentAiAssistant: ai);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);
        var contentId = Guid.NewGuid();

        var method = typeof(ContentManagementController).GetMethod(actionName);
        Assert.NotNull(method);
        var task = (Task<ActionResult<ContentAiResultDto>>)method!.Invoke(
            controller,
            [contentId, CancellationToken.None])!;
        var result = await task;

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedOperation, ai.LastOperation);
        Assert.Equal(contentId, ai.LastContentId);
    }

    [Fact]
    public async Task UpsertArticleMetadata_creates_when_missing()
    {
        var article = new FakeArticleMetadataService();
        var controller = CreateManagementController(
            new FakeContentService(),
            articleMetadataService: article);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var contentId = Guid.NewGuid();
        var request = new UpdateArticleMetadataRequest
        {
            DifficultyLevel = "Intermediate",
            ReadingTimeMinutes = 8,
            IsFeatured = true,
            AllowComments = true,
            TableOfContentsEnabled = true,
        };

        var result = await controller.UpsertArticleMetadata(contentId, request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(nameof(IArticleMetadataService.CreateAsync), article.LastOperation);
        Assert.Equal(contentId, article.LastContentId);
        Assert.Equal(userId, article.LastActor!.UserId);
        Assert.Same(request, article.LastRequest);
    }

    [Fact]
    public async Task UpsertArticleMetadata_updates_when_exists()
    {
        var contentId = Guid.NewGuid();
        var article = new FakeArticleMetadataService
        {
            MetadataToReturn = new ArticleMetadataDto(
                Guid.NewGuid(),
                contentId,
                null,
                "Beginner",
                5,
                false,
                true,
                true,
                DateTime.UtcNow,
                DateTime.UtcNow),
        };
        var controller = CreateManagementController(
            new FakeContentService(),
            articleMetadataService: article);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        var result = await controller.UpsertArticleMetadata(
            contentId,
            new UpdateArticleMetadataRequest { ReadingTimeMinutes = 12, DifficultyLevel = "Advanced" },
            CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(nameof(IArticleMetadataService.UpdateAsync), article.LastOperation);
    }

    [Fact]
    public async Task GetArticleMetadata_returns_no_content_when_missing()
    {
        var controller = CreateManagementController(new FakeContentService());
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Writer);

        var result = await controller.GetArticleMetadata(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task UpsertNewsMetadata_creates_when_missing()
    {
        var news = new FakeNewsMetadataService();
        var controller = CreateManagementController(
            new FakeContentService(),
            newsMetadataService: news);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var contentId = Guid.NewGuid();
        var request = new UpdateNewsMetadataRequest
        {
            SourceName = "HelpDev Wire",
            SourceUrl = "https://helpdev.example/source",
            Priority = "Breaking",
            NewsDateUtc = DateTime.UtcNow,
        };

        var result = await controller.UpsertNewsMetadata(contentId, request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(nameof(INewsMetadataService.CreateAsync), news.LastOperation);
        Assert.Equal(contentId, news.LastContentId);
        Assert.Same(request, news.LastRequest);
    }

    [Fact]
    public void Article_and_news_metadata_dto_contracts_are_stable()
    {
        Assert.Equal(
            new[]
            {
                "Id",
                "ContentId",
                "CategoryId",
                "DifficultyLevel",
                "ReadingTimeMinutes",
                "IsFeatured",
                "AllowComments",
                "TableOfContentsEnabled",
                "CreatedAtUtc",
                "UpdatedAtUtc",
            },
            typeof(ArticleMetadataDto).GetProperties().Select(p => p.Name).ToArray());

        Assert.Equal(
            new[]
            {
                "Id",
                "ContentId",
                "SourceName",
                "SourceUrl",
                "NewsDateUtc",
                "Priority",
                "ExternalReference",
                "CreatedAtUtc",
                "UpdatedAtUtc",
            },
            typeof(NewsMetadataDto).GetProperties().Select(p => p.Name).ToArray());
    }

    [Fact]
    public void Controller_exposes_article_news_and_tool_services()
    {
        var ctor = typeof(ContentManagementController).GetConstructors().Single();
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IArticleMetadataService));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(INewsMetadataService));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IToolService));
    }

    [Fact]
    public async Task UpsertTool_creates_when_missing()
    {
        var tools = new FakeToolService();
        var controller = CreateManagementController(new FakeContentService(), toolService: tools);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var contentId = Guid.NewGuid();
        var request = new UpdateToolRequest
        {
            ToolName = "Cursor",
            OfficialWebsiteUrl = "https://cursor.com",
            ToolCategory = "IDE",
            PricingModel = "Freemium",
            LicenseType = "Commercial",
            Platforms = ["Windows", "MacOS", "Web"],
        };

        var result = await controller.UpsertTool(contentId, request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(nameof(IToolService.CreateAsync), tools.LastOperation);
        Assert.Equal(contentId, tools.LastContentId);
        Assert.Same(request, tools.LastRequest);
    }

    [Fact]
    public void Tool_detail_dto_contract_is_stable()
    {
        Assert.Contains(typeof(ToolDetailDto).GetProperties().Select(p => p.Name), name => name == "ToolName");
        Assert.Contains(typeof(ToolDetailDto).GetProperties().Select(p => p.Name), name => name == "Features");
        Assert.Contains(typeof(ToolDetailDto).GetProperties().Select(p => p.Name), name => name == "Alternatives");
        Assert.Contains(typeof(ToolFeatureDto).GetProperties().Select(p => p.Name), name => name == "Title");
    }

    [Fact]
    public async Task UpsertRoadmap_creates_when_missing()
    {
        var roadmaps = new FakeRoadmapService();
        var controller = CreateManagementController(new FakeContentService(), roadmapService: roadmaps);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Writer);
        var contentId = Guid.NewGuid();
        var request = new UpdateRoadmapRequest
        {
            Level = "Beginner",
            EstimatedDuration = "12 weeks",
            Goal = "Become a frontend developer",
            Prerequisites = "Basic computer skills",
        };

        var result = await controller.UpsertRoadmap(contentId, request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(nameof(IRoadmapService.CreateAsync), roadmaps.LastOperation);
        Assert.Equal(contentId, roadmaps.LastContentId);
        Assert.Same(request, roadmaps.LastRequest);
    }

    private static ContentManagementController CreateManagementController(
        IContentService contentService,
        IAdminContentQueries? queries = null,
        IContentRevisionQueries? revisionQueries = null,
        IContentRevisionService? revisionService = null,
        IContentWorkflowService? workflowService = null,
        IContentAiAssistantService? contentAiAssistant = null,
        IArticleMetadataService? articleMetadataService = null,
        INewsMetadataService? newsMetadataService = null,
        IToolService? toolService = null,
        IToolAiAssistantService? toolAiAssistant = null,
        IRoadmapService? roadmapService = null,
        IRoadmapAiAssistantService? roadmapAiAssistant = null) =>
        new(
            contentService,
            queries ?? new FakeAdminContentQueries(),
            revisionQueries ?? new FakeContentRevisionQueries(),
            revisionService ?? new FakeContentRevisionService(),
            workflowService ?? new FakeContentWorkflowService(),
            contentAiAssistant ?? new FakeContentAiAssistantService(),
            articleMetadataService ?? new FakeArticleMetadataService(),
            newsMetadataService ?? new FakeNewsMetadataService(),
            toolService ?? new FakeToolService(),
            toolAiAssistant ?? new FakeToolAiAssistantService(),
            roadmapService ?? new FakeRoadmapService(),
            roadmapAiAssistant ?? new FakeRoadmapAiAssistantService());
}

public sealed class ContentExceptionFilterTests
{
    [Theory]
    [InlineData(ContentErrorCodes.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ContentErrorCodes.SlugDuplicate, StatusCodes.Status409Conflict)]
    [InlineData(ContentErrorCodes.OperationInvalid, StatusCodes.Status409Conflict)]
    [InlineData(ContentErrorCodes.Validation, StatusCodes.Status400BadRequest)]
    public void Filter_maps_content_exception_codes_to_status(string code, int expectedStatus)
    {
        var filter = new ContentExceptionFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = new ContentException("boom", code),
        };

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(expectedStatus, result.StatusCode);
    }

    [Theory]
    [InlineData(ContentAiErrorCodes.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ContentAiErrorCodes.Disabled, StatusCodes.Status403Forbidden)]
    [InlineData(ContentAiErrorCodes.TaskNotAllowed, StatusCodes.Status403Forbidden)]
    [InlineData(ContentAiErrorCodes.ProviderFailed, StatusCodes.Status502BadGateway)]
    public void Filter_maps_content_ai_exception_codes_to_status(string code, int expectedStatus)
    {
        var filter = new ContentExceptionFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = new ContentAiException("boom", code),
        };

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(expectedStatus, result.StatusCode);
    }

    [Fact]
    public void Filter_ignores_non_content_exceptions()
    {
        var filter = new ContentExceptionFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = new InvalidOperationException("other"),
        };

        filter.OnException(context);

        Assert.False(context.ExceptionHandled);
        Assert.Null(context.Result);
    }
}
