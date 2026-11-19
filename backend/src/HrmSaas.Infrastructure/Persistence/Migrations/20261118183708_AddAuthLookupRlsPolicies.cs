using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthLookupRlsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsMigrationSql.GetAuthLookupRlsUpdateSql());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsMigrationSql.GetAuthLookupRlsRollbackSql());
        }
    }
}
