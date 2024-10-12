using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UbaClone.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedMoreFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "ubaClones");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "ubaClones");

            migrationBuilder.DropColumn(
                name: "Pin",
                table: "ubaClones");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "ubaClones",
                newName: "Contact");

            migrationBuilder.AlterColumn<double>(
                name: "Balance",
                table: "ubaClones",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordHash",
                table: "ubaClones",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordSalt",
                table: "ubaClones",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PinHash",
                table: "ubaClones",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PinSalt",
                table: "ubaClones",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "ubaClones");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "ubaClones");

            migrationBuilder.DropColumn(
                name: "PinHash",
                table: "ubaClones");

            migrationBuilder.DropColumn(
                name: "PinSalt",
                table: "ubaClones");

            migrationBuilder.RenameColumn(
                name: "Contact",
                table: "ubaClones",
                newName: "Password");

            migrationBuilder.AlterColumn<int>(
                name: "Balance",
                table: "ubaClones",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "ubaClones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "ubaClones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Pin",
                table: "ubaClones",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
