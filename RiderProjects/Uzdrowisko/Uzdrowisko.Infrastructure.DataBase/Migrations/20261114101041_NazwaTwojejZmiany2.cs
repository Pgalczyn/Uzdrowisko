using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Uzdrowisko.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class NazwaTwojejZmiany2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredRoleId",
                table: "Treatments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    RoleDescription = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRoles",
                columns: table => new
                {
                    EmployeesCollectionId = table.Column<int>(type: "integer", nullable: false),
                    RolesCollectionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeRoles", x => new { x.EmployeesCollectionId, x.RolesCollectionId });
                    table.ForeignKey(
                        name: "FK_EmployeeRoles_Employees_EmployeesCollectionId",
                        column: x => x.EmployeesCollectionId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeRoles_Roles_RolesCollectionId",
                        column: x => x.RolesCollectionId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Treatments_RequiredRoleId",
                table: "Treatments",
                column: "RequiredRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRoles_RolesCollectionId",
                table: "EmployeeRoles",
                column: "RolesCollectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Treatments_Roles_RequiredRoleId",
                table: "Treatments",
                column: "RequiredRoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Treatments_Roles_RequiredRoleId",
                table: "Treatments");

            migrationBuilder.DropTable(
                name: "EmployeeRoles");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Treatments_RequiredRoleId",
                table: "Treatments");

            migrationBuilder.DropColumn(
                name: "RequiredRoleId",
                table: "Treatments");
        }
    }
}
