using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomemadeFood.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerComplianceInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProducerComplianceProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProducerProfileId = table.Column<int>(type: "int", nullable: false),
                    TaxStatus = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValue: "NotDeclared")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxNumber = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxExemptionCertificateNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FoodBusinessRegistrationNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FoodRegistrationStatus = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValue: "NotDeclared")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentAccountStatus = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValue: "NotConfigured")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentAccountReference = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ComplianceStatus = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValue: "InfrastructureOnly")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ReviewNote = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerComplianceProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerComplianceProfiles_ProducerProfiles_ProducerProfileId",
                        column: x => x.ProducerProfileId,
                        principalTable: "ProducerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProducerDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProducerProfileId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentNumber = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StorageReference = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VerificationStatus = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValue: "NotVerified")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IssuedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ReviewNote = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerDocuments_ProducerProfiles_ProducerProfileId",
                        column: x => x.ProducerProfileId,
                        principalTable: "ProducerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProducerInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProducerProfileId = table.Column<int>(type: "int", nullable: false),
                    AuthorityType = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InspectionType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InspectionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Result = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValue: "Pending")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentReference = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecordedByAdminId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerInspections_ProducerProfiles_ProducerProfileId",
                        column: x => x.ProducerProfileId,
                        principalTable: "ProducerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerComplianceProfiles_ProducerProfileId",
                table: "ProducerComplianceProfiles",
                column: "ProducerProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProducerDocuments_ProducerProfileId_DocumentType_IsActive",
                table: "ProducerDocuments",
                columns: new[] { "ProducerProfileId", "DocumentType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerDocuments_VerificationStatus",
                table: "ProducerDocuments",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerInspections_ProducerProfileId_InspectionDate",
                table: "ProducerInspections",
                columns: new[] { "ProducerProfileId", "InspectionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerInspections_Result",
                table: "ProducerInspections",
                column: "Result");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProducerComplianceProfiles");

            migrationBuilder.DropTable(
                name: "ProducerDocuments");

            migrationBuilder.DropTable(
                name: "ProducerInspections");
        }
    }
}
