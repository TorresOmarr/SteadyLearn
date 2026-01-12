using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.src.Data.Migrations
{
    /// <inheritdoc />
    public partial class First_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    role = table.Column<int>(type: "integer", nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    password_reset_token_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    password_reset_token_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    email_verification_token_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    email_verification_token_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    family = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_expires_at",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_family",
                table: "refresh_tokens",
                columns: new[] { "user_id", "family" });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_status",
                table: "refresh_tokens",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Unique",
                table: "users",
                column: "email",
                unique: true,
                filter: "\"deleted_at\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
