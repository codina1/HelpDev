using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Content.Application.Articles;
using HelpDev.Modules.Content.Application.Articles.Dtos;
using HelpDev.Modules.Content.Application.Common;
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
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

/// <summary>
/// Admin CMS content management. Routed under /api/v1/admin/content.
/// Uses the WriterOrAdmin policy with ownership enforcement (writers manage only their own content).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Content)]
[Route("api/admin/content")]
[Route("api/v{version:apiVersion}/admin/content")]
[Authorize(Policy = AuthorizationPolicies.WriterOrAdmin)]
public sealed class ContentManagementController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly IAdminContentQueries _adminContentQueries;
    private readonly IContentRevisionQueries _revisionQueries;
    private readonly IContentRevisionService _revisionService;
    private readonly IContentWorkflowService _workflowService;
    private readonly IContentAiAssistantService _contentAiAssistant;
    private readonly IArticleMetadataService _articleMetadataService;
    private readonly INewsMetadataService _newsMetadataService;
    private readonly IToolService _toolService;
    private readonly IToolAiAssistantService _toolAiAssistant;
    private readonly IRoadmapService _roadmapService;
    private readonly IRoadmapAiAssistantService _roadmapAiAssistant;

    public ContentManagementController(
        IContentService contentService,
        IAdminContentQueries adminContentQueries,
        IContentRevisionQueries revisionQueries,
        IContentRevisionService revisionService,
        IContentWorkflowService workflowService,
        IContentAiAssistantService contentAiAssistant,
        IArticleMetadataService articleMetadataService,
        INewsMetadataService newsMetadataService,
        IToolService toolService,
        IToolAiAssistantService toolAiAssistant,
        IRoadmapService roadmapService,
        IRoadmapAiAssistantService roadmapAiAssistant)
    {
        _contentService = contentService;
        _adminContentQueries = adminContentQueries;
        _revisionQueries = revisionQueries;
        _revisionService = revisionService;
        _workflowService = workflowService;
        _contentAiAssistant = contentAiAssistant;
        _articleMetadataService = articleMetadataService;
        _newsMetadataService = newsMetadataService;
        _toolService = toolService;
        _toolAiAssistant = toolAiAssistant;
        _roadmapService = roadmapService;
        _roadmapAiAssistant = roadmapAiAssistant;
    }

    [HttpGet]
    [OpenApiOperationId("ContentManagement_List")]
    [OpenApiSummary("List content", "Lists content for admin CMS management with pagination, search and filters.")]
    [ProducesResponseType(typeof(PagedResult<AdminContentListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AdminContentListItemDto>>> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        // Writers are scoped to their own content; admins see everything.
        var authorId = actor.CanManageAllContent ? (Guid?)null : actor.UserId;
        var filter = ContentSearchFilter.Create(search, status, type, page, pageSize, authorId);

        var result = await _adminContentQueries.ListAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("ContentManagement_GetById")]
    [OpenApiSummary("Get content by id", "Returns the full admin content detail (body, excerpt, cover, SEO, timestamps). Writers may only read their own content.")]
    [ProducesResponseType(typeof(AdminContentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminContentDetailDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var content = await _contentService.GetManagedByIdAsync(actor, id, cancellationToken);
        return Ok(content);
    }

    [HttpPut("{id:guid}")]
    [OpenApiOperationId("ContentManagement_Update")]
    [OpenApiSummary("Update content", "Updates an existing content item. Writers may only update their own content.")]
    [ProducesResponseType(typeof(AdminContentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminContentDetailDto>> Update(
        Guid id,
        [FromBody] UpdateContentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var content = await _contentService.UpdateAsync(actor, id, request, cancellationToken);
        return Ok(content);
    }

    [HttpPost("{id:guid}/submit-review")]
    [OpenApiOperationId("ContentManagement_SubmitReview")]
    [OpenApiSummary("Submit for review", "Moves content from Draft to ReviewPending.")]
    [ProducesResponseType(typeof(AdminContentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminContentDetailDto>> SubmitReview(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.SubmitForReviewAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/approve")]
    [OpenApiOperationId("ContentManagement_Approve")]
    [OpenApiSummary("Approve content", "Admin: ReviewPending → Approved.")]
    [ProducesResponseType(typeof(AdminContentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminContentDetailDto>> Approve(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.ApproveAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/reject")]
    [OpenApiOperationId("ContentManagement_Reject")]
    [OpenApiSummary("Reject content", "Admin: ReviewPending → Draft with required comment.")]
    [ProducesResponseType(typeof(AdminContentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminContentDetailDto>> Reject(
        Guid id,
        [FromBody] RejectContentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.RejectAsync(actor, id, request, cancellationToken));
    }

    [HttpPost("{id:guid}/publish")]
    [OpenApiOperationId("ContentManagement_Publish")]
    [OpenApiSummary("Publish content", "Admin: Approved → Published. Already-published content is a no-op.")]
    [ProducesResponseType(typeof(AdminContentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminContentDetailDto>> Publish(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var content = await _workflowService.PublishAsync(actor, id, cancellationToken);
        return Ok(content);
    }

    [HttpPost("{id:guid}/archive")]
    [OpenApiOperationId("ContentManagement_Archive")]
    [OpenApiSummary("Archive content", "Admin: Published → Archived.")]
    [ProducesResponseType(typeof(AdminContentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminContentDetailDto>> Archive(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.ArchiveAsync(actor, id, cancellationToken));
    }

    [HttpGet("{id:guid}/workflow-history")]
    [OpenApiOperationId("ContentManagement_WorkflowHistory")]
    [OpenApiSummary("Workflow history", "Immutable transition timeline for a content item.")]
    [ProducesResponseType(typeof(WorkflowHistoryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkflowHistoryDto>> WorkflowHistory(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.GetWorkflowHistoryAsync(actor, id, cancellationToken));
    }

    [HttpPut("{id:guid}/seo")]
    [OpenApiOperationId("ContentManagement_UpdateSeo")]
    [OpenApiSummary("Update SEO metadata", "Updates SEO metadata for a content item. Writers may only update their own content.")]
    [ProducesResponseType(typeof(AdminContentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminContentDetailDto>> UpdateSeo(
        Guid id,
        [FromBody] UpdateSeoMetadataRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var content = await _contentService.UpdateSeoMetadataAsync(actor, id, request, cancellationToken);
        return Ok(content);
    }

    /// <summary>
    /// Explicit computation command: deterministic SEO analysis of saved server content.
    /// Chosen as POST (not GET) because analysis is an on-demand computation, not a
    /// stored resource — even though it is side-effect free (no DB write).
    /// </summary>
    [HttpPost("{id:guid}/seo-analysis")]
    [OpenApiOperationId("ContentManagement_AnalyzeSeo")]
    [OpenApiSummary("Analyze SEO", "Runs the deterministic, rule-based SEO analyzer on saved content. No persistence.")]
    [ProducesResponseType(typeof(SeoAuditReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeoAuditReportDto>> AnalyzeSeo(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var report = await _contentService.AnalyzeSeoAsync(actor, id, cancellationToken);
        return Ok(report);
    }

    [HttpPost("{id:guid}/ai/analyze")]
    [OpenApiOperationId("ContentManagement_AiAnalyze")]
    [OpenApiSummary("AI content analysis", "On-demand AI suggestion. Does not auto-save. Human approval required.")]
    [ProducesResponseType(typeof(ContentAiResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ContentAiResultDto>> AiAnalyze(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _contentAiAssistant.AnalyzeContentAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/ai/title-suggestions")]
    [OpenApiOperationId("ContentManagement_AiTitleSuggestions")]
    [OpenApiSummary("AI title suggestions", "On-demand AI suggestion. Does not auto-save.")]
    [ProducesResponseType(typeof(ContentAiResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ContentAiResultDto>> AiTitleSuggestions(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _contentAiAssistant.GenerateTitleSuggestionsAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/ai/meta-description")]
    [OpenApiOperationId("ContentManagement_AiMetaDescription")]
    [OpenApiSummary("AI meta description", "On-demand AI suggestion. Does not auto-save.")]
    [ProducesResponseType(typeof(ContentAiResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ContentAiResultDto>> AiMetaDescription(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _contentAiAssistant.GenerateMetaDescriptionAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/ai/outline")]
    [OpenApiOperationId("ContentManagement_AiOutline")]
    [OpenApiSummary("AI outline", "On-demand AI suggestion. Does not auto-save.")]
    [ProducesResponseType(typeof(ContentAiResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ContentAiResultDto>> AiOutline(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _contentAiAssistant.GenerateOutlineAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/ai/faq")]
    [OpenApiOperationId("ContentManagement_AiFaq")]
    [OpenApiSummary("AI FAQ", "On-demand AI suggestion. Does not auto-save.")]
    [ProducesResponseType(typeof(ContentAiResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ContentAiResultDto>> AiFaq(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _contentAiAssistant.GenerateFaqAsync(actor, id, cancellationToken));
    }

    [HttpGet("{id:guid}/revisions")]
    [OpenApiOperationId("ContentManagement_ListRevisions")]
    [OpenApiSummary("List content revisions", "Paginated revision history for a content item (newest first).")]
    [ProducesResponseType(typeof(PagedResult<ContentRevisionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ContentRevisionListItemDto>>> ListRevisions(
        Guid id,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _revisionQueries.GetPagedAsync(
            actor,
            id,
            page ?? 1,
            pageSize ?? 20,
            cancellationToken);
        return Ok(result);
    }

    // Route param must not be named "version" — that collides with {version:apiVersion} on the controller.
    [HttpGet("{id:guid}/revisions/{revisionVersion:int:min(1)}")]
    [OpenApiOperationId("ContentManagement_GetRevision")]
    [OpenApiSummary("Get content revision", "Returns a single immutable revision snapshot.")]
    [ProducesResponseType(typeof(ContentRevisionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentRevisionDetailDto>> GetRevision(
        Guid id,
        int revisionVersion,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var detail = await _revisionQueries.GetByVersionAsync(actor, id, revisionVersion, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        return Ok(detail);
    }

    [HttpPost("{id:guid}/revisions/{revisionVersion:int:min(1)}/restore")]
    [OpenApiOperationId("ContentManagement_RestoreRevision")]
    [OpenApiSummary("Restore content revision", "Applies a revision snapshot and appends a new revision.")]
    [ProducesResponseType(typeof(AdminContentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminContentDetailDto>> RestoreRevision(
        Guid id,
        int revisionVersion,
        [FromBody] RestoreContentRevisionRequest? request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var content = await _revisionService.RestoreAsync(actor, id, revisionVersion, request, cancellationToken);
        return Ok(content);
    }

    [HttpGet("{id:guid}/article")]
    [OpenApiOperationId("ContentManagement_GetArticleMetadata")]
    [OpenApiSummary("Get article metadata", "Returns article-specific metadata for a content item. Writers may only access their own content.")]
    [ProducesResponseType(typeof(ArticleMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleMetadataDto>> GetArticleMetadata(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var metadata = await _articleMetadataService.GetByContentIdAsync(actor, id, cancellationToken);
        if (metadata is null)
        {
            return NoContent();
        }

        return Ok(metadata);
    }

    [HttpPut("{id:guid}/article")]
    [OpenApiOperationId("ContentManagement_UpsertArticleMetadata")]
    [OpenApiSummary("Create or update article metadata", "Upserts article settings. Writers may only update their own Article content.")]
    [ProducesResponseType(typeof(ArticleMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArticleMetadataDto>> UpsertArticleMetadata(
        Guid id,
        [FromBody] UpdateArticleMetadataRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var existing = await _articleMetadataService.GetByContentIdAsync(actor, id, cancellationToken);
        var metadata = existing is null
            ? await _articleMetadataService.CreateAsync(actor, id, request, cancellationToken)
            : await _articleMetadataService.UpdateAsync(actor, id, request, cancellationToken);
        return Ok(metadata);
    }

    [HttpGet("{id:guid}/news")]
    [OpenApiOperationId("ContentManagement_GetNewsMetadata")]
    [OpenApiSummary("Get news metadata", "Returns news-specific metadata for a content item. Writers may only access their own content.")]
    [ProducesResponseType(typeof(NewsMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NewsMetadataDto>> GetNewsMetadata(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var metadata = await _newsMetadataService.GetByContentIdAsync(actor, id, cancellationToken);
        if (metadata is null)
        {
            return NoContent();
        }

        return Ok(metadata);
    }

    [HttpPut("{id:guid}/news")]
    [OpenApiOperationId("ContentManagement_UpsertNewsMetadata")]
    [OpenApiSummary("Create or update news metadata", "Upserts news settings. Writers may only update their own News content.")]
    [ProducesResponseType(typeof(NewsMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NewsMetadataDto>> UpsertNewsMetadata(
        Guid id,
        [FromBody] UpdateNewsMetadataRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var existing = await _newsMetadataService.GetByContentIdAsync(actor, id, cancellationToken);
        var metadata = existing is null
            ? await _newsMetadataService.CreateAsync(actor, id, request, cancellationToken)
            : await _newsMetadataService.UpdateAsync(actor, id, request, cancellationToken);
        return Ok(metadata);
    }

    [HttpGet("{id:guid}/tool")]
    [OpenApiOperationId("ContentManagement_GetTool")]
    [OpenApiSummary("Get tool metadata", "Returns tool library metadata for a ContentType=Tool item.")]
    [ProducesResponseType(typeof(ToolDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToolDetailDto>> GetTool(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var tool = await _toolService.GetByContentIdAsync(actor, id, cancellationToken);
        if (tool is null)
        {
            return NoContent();
        }

        return Ok(tool);
    }

    [HttpPut("{id:guid}/tool")]
    [OpenApiOperationId("ContentManagement_UpsertTool")]
    [OpenApiSummary("Create or update tool metadata", "Upserts tool catalog fields and alternatives.")]
    [ProducesResponseType(typeof(ToolDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ToolDetailDto>> UpsertTool(
        Guid id,
        [FromBody] UpdateToolRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var existing = await _toolService.GetByContentIdAsync(actor, id, cancellationToken);
        var tool = existing is null
            ? await _toolService.CreateAsync(actor, id, request, cancellationToken)
            : await _toolService.UpdateAsync(actor, id, request, cancellationToken);
        return Ok(tool);
    }

    [HttpPost("{id:guid}/tool/features")]
    [OpenApiOperationId("ContentManagement_AddToolFeature")]
    [OpenApiSummary("Add tool feature", "Adds a feature row to the tool satellite.")]
    [ProducesResponseType(typeof(ToolFeatureDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToolFeatureDto>> AddToolFeature(
        Guid id,
        [FromBody] CreateToolFeatureRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var feature = await _toolService.AddFeatureAsync(actor, id, request, cancellationToken);
        return Ok(feature);
    }

    [HttpDelete("{id:guid}/tool/features/{featureId:guid}")]
    [OpenApiOperationId("ContentManagement_RemoveToolFeature")]
    [OpenApiSummary("Remove tool feature", "Deletes a feature from the tool satellite.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveToolFeature(
        Guid id,
        Guid featureId,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        await _toolService.RemoveFeatureAsync(actor, id, featureId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/tool/ai/summary")]
    [OpenApiOperationId("ContentManagement_ToolAiSummary")]
    [OpenApiSummary("Tool AI summary suggestion", "Suggestion only — never auto-saves.")]
    [ProducesResponseType(typeof(ToolAiSuggestionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ToolAiSuggestionDto>> ToolAiSummary(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _toolAiAssistant.SuggestSummaryAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/tool/ai/features")]
    [OpenApiOperationId("ContentManagement_ToolAiFeatures")]
    [OpenApiSummary("Tool AI feature extraction suggestion", "Suggestion only — never auto-saves.")]
    [ProducesResponseType(typeof(ToolAiSuggestionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ToolAiSuggestionDto>> ToolAiFeatures(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _toolAiAssistant.SuggestFeaturesAsync(actor, id, cancellationToken));
    }

    [HttpGet("{id:guid}/roadmap")]
    [OpenApiOperationId("ContentManagement_GetRoadmap")]
    [OpenApiSummary("Get roadmap metadata", "Returns roadmap engine metadata for a ContentType=Roadmap item.")]
    [ProducesResponseType(typeof(RoadmapDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<RoadmapDetailDto>> GetRoadmap(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var roadmap = await _roadmapService.GetByContentIdAsync(actor, id, cancellationToken);
        if (roadmap is null)
        {
            return NoContent();
        }

        return Ok(roadmap);
    }

    [HttpPut("{id:guid}/roadmap")]
    [OpenApiOperationId("ContentManagement_UpsertRoadmap")]
    [OpenApiSummary("Create or update roadmap metadata", "Upserts roadmap level, duration, goal and prerequisites.")]
    [ProducesResponseType(typeof(RoadmapDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoadmapDetailDto>> UpsertRoadmap(
        Guid id,
        [FromBody] UpdateRoadmapRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var existing = await _roadmapService.GetByContentIdAsync(actor, id, cancellationToken);
        var roadmap = existing is null
            ? await _roadmapService.CreateAsync(actor, id, request, cancellationToken)
            : await _roadmapService.UpdateAsync(actor, id, request, cancellationToken);
        return Ok(roadmap);
    }

    [HttpPost("{id:guid}/roadmap/steps")]
    [OpenApiOperationId("ContentManagement_AddRoadmapStep")]
    [OpenApiSummary("Add roadmap step", "Adds a phase/step with optional topics and resources.")]
    [ProducesResponseType(typeof(RoadmapStepDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoadmapStepDto>> AddRoadmapStep(
        Guid id,
        [FromBody] CreateRoadmapStepRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _roadmapService.AddStepAsync(actor, id, request, cancellationToken));
    }

    [HttpPut("{id:guid}/roadmap/steps/{stepId:guid}")]
    [OpenApiOperationId("ContentManagement_UpdateRoadmapStep")]
    [OpenApiSummary("Update roadmap step", "Updates a phase including topics and resources when provided.")]
    [ProducesResponseType(typeof(RoadmapStepDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoadmapStepDto>> UpdateRoadmapStep(
        Guid id,
        Guid stepId,
        [FromBody] UpdateRoadmapStepRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _roadmapService.UpdateStepAsync(actor, id, stepId, request, cancellationToken));
    }

    [HttpDelete("{id:guid}/roadmap/steps/{stepId:guid}")]
    [OpenApiOperationId("ContentManagement_RemoveRoadmapStep")]
    [OpenApiSummary("Remove roadmap step", "Deletes a phase and its topics/resources.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRoadmapStep(
        Guid id,
        Guid stepId,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        await _roadmapService.RemoveStepAsync(actor, id, stepId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/roadmap/steps/reorder")]
    [OpenApiOperationId("ContentManagement_ReorderRoadmapSteps")]
    [OpenApiSummary("Reorder roadmap steps", "Sets step order from a complete ordered id list.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReorderRoadmapSteps(
        Guid id,
        [FromBody] ReorderRoadmapStepsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        await _roadmapService.ReorderStepsAsync(actor, id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/roadmap/ai/outline")]
    [OpenApiOperationId("ContentManagement_RoadmapAiOutline")]
    [OpenApiSummary("Roadmap AI outline suggestion", "Suggestion only — never auto-creates.")]
    [ProducesResponseType(typeof(RoadmapAiSuggestionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoadmapAiSuggestionDto>> RoadmapAiOutline(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _roadmapAiAssistant.SuggestOutlineAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/roadmap/ai/phases")]
    [OpenApiOperationId("ContentManagement_RoadmapAiPhases")]
    [OpenApiSummary("Roadmap AI phases suggestion", "Suggestion only — never auto-creates.")]
    [ProducesResponseType(typeof(RoadmapAiSuggestionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoadmapAiSuggestionDto>> RoadmapAiPhases(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _roadmapAiAssistant.SuggestPhasesAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/roadmap/ai/topics")]
    [OpenApiOperationId("ContentManagement_RoadmapAiTopics")]
    [OpenApiSummary("Roadmap AI topics suggestion", "Suggestion only — never auto-creates.")]
    [ProducesResponseType(typeof(RoadmapAiSuggestionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoadmapAiSuggestionDto>> RoadmapAiTopics(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _roadmapAiAssistant.SuggestTopicsAsync(actor, id, cancellationToken));
    }

    private bool TryResolveActor(
        out ContentManagementActor actor,
        out ActionResult unauthorized)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            actor = null!;
            unauthorized = Unauthorized();
            return false;
        }

        actor = new ContentManagementActor(
            userId.Value,
            canManageAllContent: User.IsInRole(AppRoles.Admin));
        unauthorized = null!;
        return true;
    }
}
