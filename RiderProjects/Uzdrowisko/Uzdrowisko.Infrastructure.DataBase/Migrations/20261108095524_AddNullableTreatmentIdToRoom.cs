using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uzdrowisko.Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class AddNullableTreatmentIdToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TreatmentId",
                table: "Rooms",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_TreatmentId",
                table: "Rooms",
                column: "TreatmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Treatments_TreatmentId",
                table: "Rooms",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Treatments_TreatmentId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_TreatmentId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "TreatmentId",
                table: "Rooms");
        }
    }
}
