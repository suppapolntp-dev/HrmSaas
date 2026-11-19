using System.Net;
using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Attendance;

public static class PunchType
{
    public const string CheckIn = "CHECK_IN";
    public const string CheckOut = "CHECK_OUT";
}

public static class PunchSource
{
    public const string Web = "WEB";
    public const string Mobile = "MOBILE";
    public const string Biometric = "BIOMETRIC";
    public const string Adjustment = "ADJUSTMENT";
}

public sealed class AttendanceLog : BaseEntity, ITenantScoped, ICompanyScoped
{
    private AttendanceLog()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid BranchId { get; private set; }
    public DateTimeOffset PunchTimeUtc { get; private set; }
    public string PunchType { get; private set; } = Attendance.PunchType.CheckIn;
    public string Source { get; private set; } = PunchSource.Web;
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public IPAddress IpAddress { get; private set; } = IPAddress.Loopback;
    public bool IsWithinGeofence { get; private set; }

    public static AttendanceLog Create(
        Guid tenantId,
        Guid companyId,
        Guid employeeId,
        Guid branchId,
        DateTimeOffset punchTimeUtc,
        string punchType,
        string source,
        decimal? latitude,
        decimal? longitude,
        IPAddress ipAddress,
        bool isWithinGeofence,
        Guid? id = null)
    {
        var log = new AttendanceLog
        {
            TenantId = tenantId,
            CompanyId = companyId,
            EmployeeId = employeeId,
            BranchId = branchId,
            PunchTimeUtc = punchTimeUtc,
            PunchType = punchType.Trim().ToUpperInvariant(),
            Source = source.Trim().ToUpperInvariant(),
            Latitude = latitude,
            Longitude = longitude,
            IpAddress = ipAddress,
            IsWithinGeofence = isWithinGeofence
        };

        if (id.HasValue)
        {
            log.Id = id.Value;
        }

        return log;
    }
}
