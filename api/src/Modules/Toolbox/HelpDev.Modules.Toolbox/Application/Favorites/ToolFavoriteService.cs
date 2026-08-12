using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain.Favorites;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Toolbox.Application.Favorites;

public sealed class ToolFavoriteService : IToolFavoriteService
{
    private readonly IToolFavoriteRepository _favoriteRepository;
    private readonly IToolDefinitionRepository _toolRepository;
    private readonly IToolFavoriteQueries _queries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<ToolFavoriteService> _logger;

    public ToolFavoriteService(
        IToolFavoriteRepository favoriteRepository,
        IToolDefinitionRepository toolRepository,
        IToolFavoriteQueries queries,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        ILogger<ToolFavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _toolRepository = toolRepository;
        _queries = queries;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task AddAsync(Guid userId, Guid toolId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ToolboxException(
                "Authentication is required to favorite a tool.",
                ToolboxApplicationErrorCodes.FavoriteRequiresAuthentication);
        }

        try
        {
            var existing = await _favoriteRepository.GetAsync(userId, toolId, cancellationToken);
            if (existing is not null)
            {
                return;
            }

            var tool = await _toolRepository.GetByIdAsync(toolId, cancellationToken);
            if (tool is null || !tool.IsPublished)
            {
                throw new ToolboxException(
                    "Tool was not found.",
                    ToolboxApplicationErrorCodes.ToolNotFound);
            }

            if (!tool.IsEnabled)
            {
                throw new ToolboxException(
                    "Tool is disabled.",
                    ToolboxApplicationErrorCodes.ToolDisabled);
            }

            var favorite = ToolFavorite.Create(Guid.NewGuid(), userId, toolId, _clock.UtcNow);
            await _favoriteRepository.AddAsync(favorite, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Toolbox favorite added. Operation={Operation} UserId={UserId} ToolId={ToolId}",
                "favorite_added",
                userId,
                toolId);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task RemoveAsync(Guid userId, Guid toolId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ToolboxException(
                "Authentication is required to remove a favorite.",
                ToolboxApplicationErrorCodes.FavoriteRequiresAuthentication);
        }

        try
        {
            var existing = await _favoriteRepository.GetAsync(userId, toolId, cancellationToken);
            if (existing is null)
            {
                return;
            }

            _favoriteRepository.Remove(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Toolbox favorite removed. Operation={Operation} UserId={UserId} ToolId={ToolId}",
                "favorite_removed",
                userId,
                toolId);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public Task<IReadOnlyList<ToolFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ToolboxException(
                "Authentication is required to list favorites.",
                ToolboxApplicationErrorCodes.FavoriteRequiresAuthentication);
        }

        return _queries.GetUserFavoritesAsync(userId, cancellationToken);
    }

    private static ToolboxException Wrap(DomainException ex) =>
        new(ex.Message, ex.Code ?? ToolboxApplicationErrorCodes.FavoriteInvalid, ex);
}
