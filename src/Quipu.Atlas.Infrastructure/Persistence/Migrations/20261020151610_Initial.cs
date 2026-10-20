using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quipu.Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    RequestHash = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecisionReasonCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessingLeaseExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ProcessingToken = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantDetails",
                columns: table => new
                {
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    MarketCode = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    NationalityCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    IdentifierType = table.Column<int>(type: "int", nullable: false),
                    IdentifierValue = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IssuingCountryCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TermsAcceptedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantDetails", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_ApplicantDetails_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StorageKey = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderCheckAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProviderReference = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    FailureCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderCheckAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderCheckAttempts_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IdNowResults",
                columns: table => new
                {
                    ProviderCheckAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentResult = table.Column<int>(type: "int", nullable: false),
                    FaceMatch = table.Column<bool>(type: "bit", nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdNowResults", x => x.ProviderCheckAttemptId);
                    table.ForeignKey(
                        name: "FK_IdNowResults_ProviderCheckAttempts_ProviderCheckAttemptId",
                        column: x => x.ProviderCheckAttemptId,
                        principalTable: "ProviderCheckAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorldCheckResults",
                columns: table => new
                {
                    ProviderCheckAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldCheckResults", x => x.ProviderCheckAttemptId);
                    table.ForeignKey(
                        name: "FK_WorldCheckResults_ProviderCheckAttempts_ProviderCheckAttemptId",
                        column: x => x.ProviderCheckAttemptId,
                        principalTable: "ProviderCheckAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorldCheckMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorldCheckResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListType = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldCheckMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorldCheckMatches_WorldCheckResults_WorldCheckResultId",
                        column: x => x.WorldCheckResultId,
                        principalTable: "WorldCheckResults",
                        principalColumn: "ProviderCheckAttemptId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_ApplicationId",
                table: "ApplicationDocuments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "UX_Applications_IdempotencyKey",
                table: "Applications",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ProviderCheckAttempts_Application_Provider_Attempt",
                table: "ProviderCheckAttempts",
                columns: new[] { "ApplicationId", "Provider", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldCheckMatches_WorldCheckResultId",
                table: "WorldCheckMatches",
                column: "WorldCheckResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicantDetails");

            migrationBuilder.DropTable(
                name: "ApplicationDocuments");

            migrationBuilder.DropTable(
                name: "IdNowResults");

            migrationBuilder.DropTable(
                name: "WorldCheckMatches");

            migrationBuilder.DropTable(
                name: "WorldCheckResults");

            migrationBuilder.DropTable(
                name: "ProviderCheckAttempts");

            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
