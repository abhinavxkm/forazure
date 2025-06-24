using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyHousingSolution.Migrations
{
    /// <inheritdoc />
    public partial class AddDeactivationReasonToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeactivationReason",
                table: "Properties",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivationReason",
                table: "Properties");
        }
    }
}
