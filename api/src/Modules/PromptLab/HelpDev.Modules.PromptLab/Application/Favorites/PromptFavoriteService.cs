using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.PromptLab.Application.Favorites;

public sealed class PromptFavoriteService : IPromptFavoriteService
{
    private readonly IPromptFavoriteRepository _favoriteRepository;
    private readonly IPromptDefinitionRepository _promptRepository;
    private readonly IPromptFavoriteQueries _queries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<PromptFavoriteService> _logger;

    public PromptFavoriteService(
        IPromptFavoriteRepository favoriteRepository,
        IPromptDefinitionRepository promptRepository,
        IPromptFavoriteQueries queries,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        ILogger<PromptFavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _promptRepository = promptRepository;
        _queries = queries;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task AddAsync(Guid userId, Guid promptId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new PromptLabException(
                "Authentication is required to favorite a prompt.",
                PromptLabApplicationErrorCodes.FavoriteRequiresAuthentication);
        }

        try
        {
            var existing = await _favoriteRepository.GetAsync(userId, promptId, cancellationToken);
            if (existing is not null)
            {
                return;
            }

            var prompt = await _promptRepository.GetByIdAsync(promptId, cancellationToken);
            if (prompt is null || !prompt.IsPublished)
            {
                throw new PromptLabException(
                    "Prompt was not found.",
                    PromptLabApplicationErrorCodes.PromptNotFound);
            }

            if (!prompt.IsEnabled)
            {
                throw new PromptLabException(
                    "Prompt is disabled.",
                    PromptLabApplicationErrorCodes.PromptDisabled);
            }

            var favorite = PromptFavorite.Create(Guid.NewGuid(), userId, promptId, _clock.UtcNow);
            await _favoriteRepository.AddAsync(favorite, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PromptLab favorite added. Operation={Operation} UserId={UserId} PromptId={PromptId}",
                "favorite_added",
                userId,
                promptId);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task RemoveAsync(Guid userId, Guid promptId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new PromptLabException(
                "Authentication is required to remove a favorite.",
                PromptLabApplicationErrorCodes.FavoriteRequiresAuthentication);
        }

        try
        {
            var existing = await _favoriteRepository.GetAsync(userId, promptId, cancellationToken);
            if (existing is null)
            {
                return;
            }

            _favoriteRepository.Remove(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PromptLab favorite removed. Operation={Operation} UserId={UserId} PromptId={PromptId}",
                "favorite_removed",
                userId,
                promptId);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public Task<IReadOnlyList<PromptFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new PromptLabException(
                "Authentication is required to list favorites.",
                PromptLabApplicationErrorCodes.FavoriteRequiresAuthentication);
        }

        return _queries.GetUserFavoritesAsync(userId, cancellationToken);
    }

    private static PromptLabException Wrap(DomainException ex) =>
        new(ex.Message, ex.Code ?? PromptLabApplicationErrorCodes.FavoriteInvalid, ex);
}
