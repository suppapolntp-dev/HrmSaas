using System.Text;

namespace HrmSaas.Infrastructure.Persistence;

public static class RlsMigrationSql
{
    public static readonly string[] TenantScopedTables =
    [
        "companies",
        "branches",
        "departments",
        "positions",
        "job_grades",
        "users",
        "user_sessions",
        "roles",
        "user_company_roles",
        "employees",
        "employment_contracts",
        "employee_reporting_lines",
        "shift_definitions",
        "shift_break_rules",
        "employee_shift_rosters",
        "attendance_logs",
        "attendance_daily_summaries",
        "leave_types",
        "leave_entitlement_policies",
        "employee_leave_balances",
        "workflow_definitions",
        "workflow_steps",
        "approval_requests",
        "approval_action_logs",
        "payroll_periods",
        "salary_components",
        "payslips",
        "payslip_items",
        "audit_logs"
    ];

    public static string GetEnableRlsSql()
    {
        var sql = new StringBuilder();

        foreach (string tableName in TenantScopedTables)
        {
            sql.AppendLine($"ALTER TABLE \"{tableName}\" ENABLE ROW LEVEL SECURITY;");
            sql.AppendLine(BuildCreatePolicySql(tableName));
        }

        return sql.ToString();
    }

    public static string GetAuthLookupRlsUpdateSql()
    {
        var sql = new StringBuilder();
        sql.AppendLine(BuildCreatePolicySql("users"));
        sql.AppendLine(BuildCreatePolicySql("user_sessions"));
        return sql.ToString();
    }

    public static string GetAuthLookupRlsRollbackSql()
    {
        var sql = new StringBuilder();
        sql.AppendLine(BuildStrictTenantPolicySql("users"));
        sql.AppendLine(BuildStrictTenantPolicySql("user_sessions"));
        return sql.ToString();
    }

    private static string BuildCreatePolicySql(string tableName)
    {
        if (tableName == "users")
        {
            return
                "DROP POLICY IF EXISTS tenant_isolation_policy ON \"users\";\n" +
                "CREATE POLICY tenant_isolation_policy ON \"users\" " +
                "USING (" +
                "tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid " +
                "OR email = NULLIF(current_setting('app.auth_lookup_email', true), '')" +
                ") " +
                "WITH CHECK (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);";
        }

        if (tableName == "user_sessions")
        {
            return
                "DROP POLICY IF EXISTS tenant_isolation_policy ON \"user_sessions\";\n" +
                "CREATE POLICY tenant_isolation_policy ON \"user_sessions\" " +
                "USING (" +
                "tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid " +
                "OR session_token_hash = NULLIF(current_setting('app.auth_lookup_token_hash', true), '')" +
                ") " +
                "WITH CHECK (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);";
        }

        return BuildStrictTenantPolicySql(tableName);
    }

    private static string BuildStrictTenantPolicySql(string tableName)
    {
        return
            $"DROP POLICY IF EXISTS tenant_isolation_policy ON \"{tableName}\";\n" +
            $"CREATE POLICY tenant_isolation_policy ON \"{tableName}\" " +
            "USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);";
    }

    public static string GetDisableRlsSql()
    {
        var sql = new StringBuilder();

        foreach (string tableName in TenantScopedTables)
        {
            sql.AppendLine($"DROP POLICY IF EXISTS tenant_isolation_policy ON \"{tableName}\";");
            sql.AppendLine($"ALTER TABLE \"{tableName}\" DISABLE ROW LEVEL SECURITY;");
        }

        return sql.ToString();
    }
}
