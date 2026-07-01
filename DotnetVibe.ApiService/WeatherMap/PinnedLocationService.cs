using System.Data;

using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.WeatherMap.Data;

using Microsoft.EntityFrameworkCore;

namespace DotnetVibe.ApiService.WeatherMap;

public sealed class PinnedLocationService(AppDbContext db)
{
    public async Task<PinnedLocationDto> CreateAsync(
        string userId,
        string name,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        GeoCoordinateValidator.Validate(latitude, longitude);
        var normalizedName = PinnedLocationNameValidator.ValidateAndNormalize(name);

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            async ct =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    ct);
                var created = await CreateCoreAsync(userId, normalizedName, latitude, longitude, ct);
                await transaction.CommitAsync(ct);
                return created;
            },
            cancellationToken);
    }

    private async Task<PinnedLocationDto> CreateCoreAsync(
        string userId,
        string normalizedName,
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var existingCount = await db.PinnedLocations
            .CountAsync(location => location.UserId == userId, cancellationToken);
        if (existingCount >= PinnedLocationLimits.MaxPerUser)
        {
            throw new PinnedLocationLimitExceededException();
        }

        var nextSortOrder = await db.PinnedLocations
            .Where(location => location.UserId == userId)
            .Select(location => (int?)location.SortOrder)
            .MaxAsync(cancellationToken) ?? -1;

        var entity = new PinnedLocation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = normalizedName,
            Latitude = latitude,
            Longitude = longitude,
            SortOrder = nextSortOrder + 1
        };

        db.PinnedLocations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<IReadOnlyList<PinnedLocationDto>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        await db.PinnedLocations
            .Where(location => location.UserId == userId)
            .OrderBy(location => location.SortOrder)
            .Select(location => new PinnedLocationDto(
                location.Id,
                location.Name,
                location.Latitude,
                location.Longitude,
                location.SortOrder))
            .ToListAsync(cancellationToken);

    public async Task<PinnedLocationDto?> UpdateAsync(
        string userId,
        Guid id,
        string? name,
        double? latitude,
        double? longitude,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PinnedLocations
            .SingleOrDefaultAsync(location => location.Id == id && location.UserId == userId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (name is not null)
        {
            entity.Name = PinnedLocationNameValidator.ValidateAndNormalize(name);
        }

        if (latitude is not null || longitude is not null)
        {
            var updatedLatitude = latitude ?? entity.Latitude;
            var updatedLongitude = longitude ?? entity.Longitude;
            GeoCoordinateValidator.Validate(updatedLatitude, updatedLongitude);
            entity.Latitude = updatedLatitude;
            entity.Longitude = updatedLongitude;
        }

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PinnedLocations
            .SingleOrDefaultAsync(location => location.Id == id && location.UserId == userId, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        db.PinnedLocations.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PinnedLocationDto ToDto(PinnedLocation entity) =>
        new(entity.Id, entity.Name, entity.Latitude, entity.Longitude, entity.SortOrder);
}
