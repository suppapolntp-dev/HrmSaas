using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Organizations;

public sealed class Branch : TenantCompanyEntity
{
    private Branch()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Timezone { get; private set; } = "Asia/Bangkok";
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public int GeofenceRadiusMeters { get; private set; }
    public string[] AllowedIps { get; private set; } = [];

    public static Branch Create(
        Guid tenantId,
        Guid companyId,
        string code,
        string name,
        string timezone,
        decimal latitude,
        decimal longitude,
        int geofenceRadiusMeters,
        string[]? allowedIps = null,
        Guid? id = null)
    {
        if (tenantId == Guid.Empty || companyId == Guid.Empty)
        {
            throw new ArgumentException("TenantId and CompanyId are required.");
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Branch code and name are required.");
        }

        if (geofenceRadiusMeters < 0)
        {
            throw new ArgumentException("Geofence radius cannot be negative.", nameof(geofenceRadiusMeters));
        }

        var branch = new Branch
        {
            TenantId = tenantId,
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Timezone = string.IsNullOrWhiteSpace(timezone) ? "Asia/Bangkok" : timezone.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            GeofenceRadiusMeters = geofenceRadiusMeters,
            AllowedIps = allowedIps ?? []
        };

        if (id.HasValue)
        {
            branch.Id = id.Value;
        }

        return branch;
    }

    public void UpdateGeofence(decimal latitude, decimal longitude, int radiusMeters, string[] allowedIps)
    {
        if (radiusMeters < 0)
        {
            throw new ArgumentException("Geofence radius cannot be negative.", nameof(radiusMeters));
        }

        Latitude = latitude;
        Longitude = longitude;
        GeofenceRadiusMeters = radiusMeters;
        AllowedIps = allowedIps;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
