using HelpDev.Application.Persistence;
using HelpDev.Application.Test.Dtos;
using HelpDev.Modules.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Persistence.Repositories;

public sealed class TestRepository : ITestRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TestRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TestUserDto>> ListUsersAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.CreatedAt)
            .Select(user => new TestUserDto(
                user.Id,
                user.Mobile,
                user.FullName,
                user.Role.ToString()))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TestContentTypeCountDto>> GetPublishedContentCountsByTypeAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = await _dbContext.Contents
            .AsNoTracking()
            .Where(content => content.Status == ContentStatus.Published)
            .GroupBy(content => content.Type)
            .Select(group => new { Type = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return groups
            .OrderBy(item => item.Type.ToString())
            .Select(item => new TestContentTypeCountDto(item.Type.ToString(), item.Count))
            .ToList();
    }
}
