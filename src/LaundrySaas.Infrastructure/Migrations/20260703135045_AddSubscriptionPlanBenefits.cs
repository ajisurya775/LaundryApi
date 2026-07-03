using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaundrySaas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlanBenefits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SubscriptionPlans",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraCredit",
                table: "SubscriptionPlans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "HasAccounting",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasInventory",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPos",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraCredit",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "HasAccounting",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "HasInventory",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "HasPos",
                table: "SubscriptionPlans");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SubscriptionPlans",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
