using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.Tools.Dtos;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.Tools;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Content.Application.Tools;

public sealed class ToolService : IToolService
{
    private readonly IContentRepository _contentRepository;
    private readonly IToolRepository _toolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public ToolService(
        IContentRepository contentRepository,
        IToolRepository toolRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _contentRepository = contentRepository;
        _toolRepository = toolRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<ToolDetailDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var content = await LoadToolContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var tool = await _toolRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        return tool is null ? null : ToolMapper.ToDetail(tool);
    }

    public async Task<ToolDetailDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateToolRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadToolContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var existing = await _toolRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            throw new ContentException("متادیتای ابزار قبلاً ایجاد شده است.", ContentErrorCodes.OperationInvalid);
        }

        try
        {
            var tool = ToolMetadata.Create(
                Guid.NewGuid(),
                content.Id,
                request.ToolName,
                request.OfficialWebsiteUrl,
                request.GithubUrl,
                request.LogoMediaId,
                request.CompanyName,
                ParsePricing(request.PricingModel),
                request.ToolCategory,
                ParsePlatforms(request.Platforms),
                ParseLicense(request.LicenseType),
                _clock.UtcNow);

            await _toolRepository.AddAsync(tool, cancellationToken).ConfigureAwait(false);

            if (request.Alternatives is not null)
            {
                ApplyAlternatives(tool, request.Alternatives, content.Id, _clock.UtcNow);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ToolMapper.ToDetail(tool);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    public async Task<ToolDetailDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateToolRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadToolContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var tool = await _toolRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (tool is null)
        {
            throw new ContentException("متادیتای ابزار یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            tool.Update(
                request.ToolName,
                request.OfficialWebsiteUrl,
                request.GithubUrl,
                request.LogoMediaId,
                request.CompanyName,
                ParsePricing(request.PricingModel),
                request.ToolCategory,
                ParsePlatforms(request.Platforms),
                ParseLicense(request.LicenseType),
                _clock.UtcNow);

            if (request.Alternatives is not null)
            {
                ApplyAlternatives(tool, request.Alternatives, content.Id, _clock.UtcNow);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ToolMapper.ToDetail(tool);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    public async Task<ToolFeatureDto> AddFeatureAsync(
        ContentManagementActor actor,
        Guid contentId,
        CreateToolFeatureRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadToolContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var tool = await _toolRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (tool is null)
        {
            throw new ContentException("متادیتای ابزار یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            var order = request.Order
                ?? await _toolRepository.GetNextFeatureOrderAsync(tool.Id, cancellationToken).ConfigureAwait(false);
            var feature = ToolFeature.Create(Guid.NewGuid(), tool.Id, request.Title, request.Description, order);
            await _toolRepository.AddFeatureAsync(feature, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ToolMapper.ToFeature(feature);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    public async Task RemoveFeatureAsync(
        ContentManagementActor actor,
        Guid contentId,
        Guid featureId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var content = await LoadToolContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var tool = await _toolRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (tool is null)
        {
            throw new ContentException("متادیتای ابزار یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            tool.RemoveFeature(featureId, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.NotFound, ex);
        }
    }

    private async Task<Domain.Entities.Content> LoadToolContentAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken).ConfigureAwait(false);
        if (content is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        ContentService.EnsureCanManage(content, actor);

        if (content.Type != ContentType.Tool)
        {
            throw new ContentException("این محتوا از نوع ابزار نیست.", ContentErrorCodes.Validation);
        }

        return content;
    }

    private static void ApplyAlternatives(
        ToolMetadata tool,
        IReadOnlyList<UpdateToolAlternativeItem>? items,
        Guid contentId,
        DateTime updatedAtUtc)
    {
        var alternatives = (items ?? [])
            .Select(item =>
            {
                if (item.AlternativeToolContentId == contentId)
                {
                    throw new DomainException("ابزار نمی‌تواند جایگزین خودش باشد.");
                }

                return ToolAlternative.Create(
                    Guid.NewGuid(),
                    tool.Id,
                    item.AlternativeToolContentId,
                    item.Order);
            })
            .ToList();

        tool.ReplaceAlternatives(alternatives, updatedAtUtc);
    }

    private static PricingModel ParsePricing(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value.Trim(), ignoreCase: true, out PricingModel model)
            || !Enum.IsDefined(model))
        {
            throw new DomainException("مدل قیمت‌گذاری معتبر نیست.");
        }

        return model;
    }

    private static LicenseType ParseLicense(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value.Trim(), ignoreCase: true, out LicenseType license)
            || !Enum.IsDefined(license))
        {
            throw new DomainException("نوع لایسنس معتبر نیست.");
        }

        return license;
    }

    private static PlatformSupport ParsePlatforms(IReadOnlyList<string>? platforms)
    {
        if (platforms is null || platforms.Count == 0)
        {
            return PlatformSupport.None;
        }

        var flags = PlatformSupport.None;
        foreach (var raw in platforms)
        {
            if (string.IsNullOrWhiteSpace(raw)
                || !Enum.TryParse(raw.Trim(), ignoreCase: true, out PlatformSupport flag)
                || flag == PlatformSupport.None
                || !IsSinglePlatformFlag(flag))
            {
                throw new DomainException("پلتفرم پشتیبانی‌شده معتبر نیست.");
            }

            flags |= flag;
        }

        return flags;
    }

    private static bool IsSinglePlatformFlag(PlatformSupport flag) =>
        flag is PlatformSupport.Windows or PlatformSupport.Linux or PlatformSupport.MacOS or PlatformSupport.Web;
}
