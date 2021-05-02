using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Domain.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactRequests",
                columns: table => new
                {
                    ContactRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(3500)", maxLength: 3500, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRequests", x => x.ContactRequestId);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Thread = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TimeMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Time = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usages",
                columns: table => new
                {
                    UsageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EventName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usages", x => x.UsageId);
                });

            migrationBuilder.CreateTable(
                name: "UserActivityLogs",
                columns: table => new
                {
                    UserActivityLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KeyColumn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KeyValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLogs", x => x.UserActivityLogId);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CurrentTagIds = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    CurrentNotifications = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    CurrentPeople = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    CurrentSuggestedPeople = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    Skip = table.Column<int>(type: "int", nullable: true),
                    Me = table.Column<bool>(type: "bit", nullable: true),
                    NotififySuggestions = table.Column<bool>(type: "bit", nullable: false),
                    PrivateMode = table.Column<bool>(type: "bit", nullable: false),
                    PremiumExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedTerms = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuggestionUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reasons = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    Token = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TokenCreation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TokenAttempts = table.Column<int>(type: "int", nullable: false),
                    GiveFeedback = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SuggestionReminderSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestionRemindersSent = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.AlbumId);
                    table.ForeignKey(
                        name: "FK_Albums_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    PromptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Relationship = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Template = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.PromptId);
                    table.ForeignKey(
                        name: "FK_Prompts_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingSuggestions",
                columns: table => new
                {
                    OwnerUserId = table.Column<int>(type: "int", nullable: false),
                    SuggestedUserId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Resolved = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolution = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingSuggestions", x => new { x.OwnerUserId, x.SuggestedUserId });
                    table.ForeignKey(
                        name: "FK_SharingSuggestions_UserProfiles_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_SharingSuggestions_UserProfiles_SuggestedUserId",
                        column: x => x.SuggestedUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Timelines",
                columns: table => new
                {
                    TimelineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timelines", x => x.TimelineId);
                    table.ForeignKey(
                        name: "FK_Timelines_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountCents = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChargeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserEmails",
                columns: table => new
                {
                    UserEmailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(70)", maxLength: 70, nullable: true),
                    Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    TokenExpiration = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEmails", x => x.UserEmailId);
                    table.ForeignKey(
                        name: "FK_UserEmails_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNetworks",
                columns: table => new
                {
                    UserTagId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsTask = table.Column<bool>(type: "bit", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNetworks", x => x.UserTagId);
                    table.ForeignKey(
                        name: "FK_UserNetworks_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRelationships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Relationship = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRelationships_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserUsers",
                columns: table => new
                {
                    UserUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerUserId = table.Column<int>(type: "int", nullable: false),
                    ReaderUserId = table.Column<int>(type: "int", nullable: false),
                    ReaderName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Archive = table.Column<bool>(type: "bit", nullable: false),
                    SendNotificationEmail = table.Column<bool>(type: "bit", nullable: false),
                    CanReShare = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUsers", x => x.UserUserId);
                    table.ForeignKey(
                        name: "FK_UserUsers_UserProfiles_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_UserUsers_UserProfiles_ReaderUserId",
                        column: x => x.ReaderUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "AlbumExports",
                columns: table => new
                {
                    AlbumExportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumPublicId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumExports", x => x.AlbumExportId);
                    table.ForeignKey(
                        name: "FK_AlbumExports_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPrompts",
                columns: table => new
                {
                    UserPromptId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PromptId = table.Column<int>(type: "int", nullable: false),
                    Used = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Dismissed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrompts", x => x.UserPromptId);
                    table.ForeignKey(
                        name: "FK_UserPrompts_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "PromptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPrompts_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Drops",
                columns: table => new
                {
                    DropId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentDropId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateType = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Completed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PromptId = table.Column<int>(type: "int", nullable: true),
                    TimelineId = table.Column<int>(type: "int", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: true),
                    DayOfYear = table.Column<int>(type: "int", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drops", x => x.DropId);
                    table.ForeignKey(
                        name: "FK_Drops_Drops_ParentDropId",
                        column: x => x.ParentDropId,
                        principalTable: "Drops",
                        principalColumn: "DropId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Drops_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "PromptId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Drops_Timelines_TimelineId",
                        column: x => x.TimelineId,
                        principalTable: "Timelines",
                        principalColumn: "TimelineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Drops_UserProfiles_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Drops_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromptTimelines",
                columns: table => new
                {
                    PromptIdTimelineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromptId = table.Column<int>(type: "int", nullable: false),
                    TimelineId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptTimelines", x => x.PromptIdTimelineId);
                    table.ForeignKey(
                        name: "FK_PromptTimelines_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "PromptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromptTimelines_Timelines_TimelineId",
                        column: x => x.TimelineId,
                        principalTable: "Timelines",
                        principalColumn: "TimelineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimelineUsers",
                columns: table => new
                {
                    TimelineUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimelineId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineUsers", x => x.TimelineUserId);
                    table.ForeignKey(
                        name: "FK_TimelineUsers_Timelines_TimelineId",
                        column: x => x.TimelineId,
                        principalTable: "Timelines",
                        principalColumn: "TimelineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimelineUsers_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PremiumPlans",
                columns: table => new
                {
                    PremiumPlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanLengthDays = table.Column<int>(type: "int", nullable: false),
                    PlanType = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtendedPlanId = table.Column<int>(type: "int", nullable: true),
                    FamilyPlanCount = table.Column<int>(type: "int", nullable: false),
                    ParentPremiumPlanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumPlans", x => x.PremiumPlanId);
                    table.ForeignKey(
                        name: "FK_PremiumPlans_PremiumPlans_ParentPremiumPlanId",
                        column: x => x.ParentPremiumPlanId,
                        principalTable: "PremiumPlans",
                        principalColumn: "PremiumPlanId");
                    table.ForeignKey(
                        name: "FK_PremiumPlans_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId");
                    table.ForeignKey(
                        name: "FK_PremiumPlans_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkViewers",
                columns: table => new
                {
                    UserTagId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkViewers", x => new { x.UserTagId, x.UserId });
                    table.ForeignKey(
                        name: "FK_NetworkViewers_UserNetworks_UserTagId",
                        column: x => x.UserTagId,
                        principalTable: "UserNetworks",
                        principalColumn: "UserTagId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkViewers_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserPromptAskers",
                columns: table => new
                {
                    UserPromptAskerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPromptId = table.Column<long>(type: "bigint", nullable: false),
                    AskerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPromptAskers", x => x.UserPromptAskerId);
                    table.ForeignKey(
                        name: "FK_UserPromptAskers_UserProfiles_AskerId",
                        column: x => x.AskerId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPromptAskers_UserPrompts_UserPromptId",
                        column: x => x.UserPromptId,
                        principalTable: "UserPrompts",
                        principalColumn: "UserPromptId");
                });

            migrationBuilder.CreateTable(
                name: "AlbumDrops",
                columns: table => new
                {
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    DropId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumDrops", x => new { x.AlbumId, x.DropId });
                    table.ForeignKey(
                        name: "FK_AlbumDrops_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumDrops_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "DropId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DropId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "DropId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ContentDrops",
                columns: table => new
                {
                    ContentDropId = table.Column<int>(type: "int", nullable: false),
                    Stuff = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentDrops", x => x.ContentDropId);
                    table.ForeignKey(
                        name: "FK_ContentDrops_Drops_ContentDropId",
                        column: x => x.ContentDropId,
                        principalTable: "Drops",
                        principalColumn: "DropId");
                });

            migrationBuilder.CreateTable(
                name: "NetworkDrops",
                columns: table => new
                {
                    TagDropId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DropId = table.Column<int>(type: "int", nullable: false),
                    UserTagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkDrops", x => x.TagDropId);
                    table.ForeignKey(
                        name: "FK_NetworkDrops_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "DropId");
                    table.ForeignKey(
                        name: "FK_NetworkDrops_UserNetworks_UserTagId",
                        column: x => x.UserTagId,
                        principalTable: "UserNetworks",
                        principalColumn: "UserTagId");
                });

            migrationBuilder.CreateTable(
                name: "SharedDropNotifications",
                columns: table => new
                {
                    SharedDropNotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SharerUserId = table.Column<int>(type: "int", nullable: false),
                    TargetUserId = table.Column<int>(type: "int", nullable: false),
                    DropId = table.Column<int>(type: "int", nullable: true),
                    TimeShared = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedDropNotifications", x => x.SharedDropNotificationId);
                    table.ForeignKey(
                        name: "FK_SharedDropNotifications_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "DropId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedDropNotifications_UserProfiles_SharerUserId",
                        column: x => x.SharerUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_SharedDropNotifications_UserProfiles_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TimelineDrops",
                columns: table => new
                {
                    TimelineId = table.Column<int>(type: "int", nullable: false),
                    DropId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineDrops", x => new { x.TimelineId, x.DropId });
                    table.ForeignKey(
                        name: "FK_TimelineDrops_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "DropId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimelineDrops_Timelines_TimelineId",
                        column: x => x.TimelineId,
                        principalTable: "Timelines",
                        principalColumn: "TimelineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimelineDrops_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserDrops",
                columns: table => new
                {
                    UserDropId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DropId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDrops", x => x.UserDropId);
                    table.ForeignKey(
                        name: "FK_UserDrops_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "DropId");
                    table.ForeignKey(
                        name: "FK_UserDrops_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SharedPlans",
                columns: table => new
                {
                    SharedPlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SharedPremiumPlanId = table.Column<int>(type: "int", nullable: false),
                    EmailSentTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<int>(type: "int", nullable: true),
                    Revoked = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedPlans", x => x.SharedPlanId);
                    table.ForeignKey(
                        name: "FK_SharedPlans_PremiumPlans_SharedPremiumPlanId",
                        column: x => x.SharedPremiumPlanId,
                        principalTable: "PremiumPlans",
                        principalColumn: "PremiumPlanId");
                    table.ForeignKey(
                        name: "FK_SharedPlans_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShareRequests",
                columns: table => new
                {
                    ShareRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterUserId = table.Column<int>(type: "int", nullable: false),
                    RequestorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetsEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetAlias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetsUserId = table.Column<int>(type: "int", nullable: false),
                    RequestKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ignored = table.Column<bool>(type: "bit", nullable: false),
                    TagsToShare = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PromptId = table.Column<int>(type: "int", nullable: true),
                    TimelineId = table.Column<int>(type: "int", nullable: true),
                    PremiumPlanId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareRequests", x => x.ShareRequestId);
                    table.ForeignKey(
                        name: "FK_ShareRequests_PremiumPlans_PremiumPlanId",
                        column: x => x.PremiumPlanId,
                        principalTable: "PremiumPlans",
                        principalColumn: "PremiumPlanId");
                    table.ForeignKey(
                        name: "FK_ShareRequests_UserProfiles_RequesterUserId",
                        column: x => x.RequesterUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_ShareRequests_UserProfiles_TargetsUserId",
                        column: x => x.TargetsUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ImageDrops",
                columns: table => new
                {
                    ImageDropId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DropId = table.Column<int>(type: "int", nullable: false),
                    CommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageDrops", x => x.ImageDropId);
                    table.ForeignKey(
                        name: "FK_ImageDrops_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImageDrops_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "DropId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieDrops",
                columns: table => new
                {
                    MovieDropId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DropId = table.Column<int>(type: "int", nullable: false),
                    CommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieDrops", x => x.MovieDropId);
                    table.ForeignKey(
                        name: "FK_MovieDrops_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovieDrops_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "DropId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumDrops_DropId",
                table: "AlbumDrops",
                column: "DropId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumExports_AlbumId",
                table: "AlbumExports",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_UserId",
                table: "Albums",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DropId",
                table: "Comments",
                column: "DropId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Drop_ParentDropId",
                table: "Drops",
                column: "ParentDropId");

            migrationBuilder.CreateIndex(
                name: "IX_Drops_CompletedByUserId",
                table: "Drops",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Drops_PromptId",
                table: "Drops",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_Drops_TimelineId",
                table: "Drops",
                column: "TimelineId");

            migrationBuilder.CreateIndex(
                name: "IX_Drops_UserId",
                table: "Drops",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageDrops_CommentId",
                table: "ImageDrops",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageDrops_DropId",
                table: "ImageDrops",
                column: "DropId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieDrops_CommentId",
                table: "MovieDrops",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieDrops_DropId",
                table: "MovieDrops",
                column: "DropId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkDrops_DropId",
                table: "NetworkDrops",
                column: "DropId");

            migrationBuilder.CreateIndex(
                name: "IX_TagDrop_UserTagId_DropId",
                table: "NetworkDrops",
                columns: new[] { "UserTagId", "DropId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NetworkViewers_UserId",
                table: "NetworkViewers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumPlan_UserId",
                table: "PremiumPlans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumPlans_ParentPremiumPlanId",
                table: "PremiumPlans",
                column: "ParentPremiumPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumPlans_TransactionId",
                table: "PremiumPlans",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_UserId",
                table: "Prompts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTimelines_PromptId_TimelineId",
                table: "PromptTimelines",
                columns: new[] { "PromptId", "TimelineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromptTimelines_TimelineId",
                table: "PromptTimelines",
                column: "TimelineId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedDropNotifications_DropId",
                table: "SharedDropNotifications",
                column: "DropId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedDropNotifications_SharerUserId",
                table: "SharedDropNotifications",
                column: "SharerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedDropNotifications_TargetUserId",
                table: "SharedDropNotifications",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedPlan_PremiumPlan",
                table: "SharedPlans",
                column: "SharedPremiumPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedPlan_UserId",
                table: "SharedPlans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareRequests_PremiumPlanId",
                table: "ShareRequests",
                column: "PremiumPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareRequests_RequesterUserId",
                table: "ShareRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareRequests_TargetsUserId",
                table: "ShareRequests",
                column: "TargetsUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingSuggestions_SuggestedUserId",
                table: "SharingSuggestions",
                column: "SuggestedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineDrops_DropId",
                table: "TimelineDrops",
                column: "DropId");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineDrops_UserId",
                table: "TimelineDrops",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Timelines_UserId",
                table: "Timelines",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineUsers_TimelineId",
                table: "TimelineUsers",
                column: "TimelineId");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineUsers_UserId_TimelineId",
                table: "TimelineUsers",
                columns: new[] { "UserId", "TimelineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDrops_DropId",
                table: "UserDrops",
                column: "DropId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDrops_UserId",
                table: "UserDrops",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEmails_UserId",
                table: "UserEmails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserId_Name",
                table: "UserNetworks",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserId_PremiumExpiration",
                table: "UserProfiles",
                columns: new[] { "UserId", "PremiumExpiration" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPromptAskers_AskerId",
                table: "UserPromptAskers",
                column: "AskerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPromptAskers_UserPromptId",
                table: "UserPromptAskers",
                column: "UserPromptId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrompt_PromptId",
                table: "UserPrompts",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrompt_UserId",
                table: "UserPrompts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrompts_LastSeen",
                table: "UserPrompts",
                column: "LastSeen");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrompts_PromptId_UserId",
                table: "UserPrompts",
                columns: new[] { "PromptId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationships_UserId_Relationship",
                table: "UserRelationships",
                columns: new[] { "UserId", "Relationship" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserUser_ReaderUserId_OwnerUserId",
                table: "UserUsers",
                columns: new[] { "ReaderUserId", "OwnerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserUsers_OwnerUserId",
                table: "UserUsers",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserUsers_ReaderUserId",
                table: "UserUsers",
                column: "ReaderUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumDrops");

            migrationBuilder.DropTable(
                name: "AlbumExports");

            migrationBuilder.DropTable(
                name: "ContactRequests");

            migrationBuilder.DropTable(
                name: "ContentDrops");

            migrationBuilder.DropTable(
                name: "ImageDrops");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "MovieDrops");

            migrationBuilder.DropTable(
                name: "NetworkDrops");

            migrationBuilder.DropTable(
                name: "NetworkViewers");

            migrationBuilder.DropTable(
                name: "PromptTimelines");

            migrationBuilder.DropTable(
                name: "SharedDropNotifications");

            migrationBuilder.DropTable(
                name: "SharedPlans");

            migrationBuilder.DropTable(
                name: "ShareRequests");

            migrationBuilder.DropTable(
                name: "SharingSuggestions");

            migrationBuilder.DropTable(
                name: "TimelineDrops");

            migrationBuilder.DropTable(
                name: "TimelineUsers");

            migrationBuilder.DropTable(
                name: "TimeMethods");

            migrationBuilder.DropTable(
                name: "Usages");

            migrationBuilder.DropTable(
                name: "UserActivityLogs");

            migrationBuilder.DropTable(
                name: "UserDrops");

            migrationBuilder.DropTable(
                name: "UserEmails");

            migrationBuilder.DropTable(
                name: "UserPromptAskers");

            migrationBuilder.DropTable(
                name: "UserRelationships");

            migrationBuilder.DropTable(
                name: "UserUsers");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "UserNetworks");

            migrationBuilder.DropTable(
                name: "PremiumPlans");

            migrationBuilder.DropTable(
                name: "UserPrompts");

            migrationBuilder.DropTable(
                name: "Drops");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Prompts");

            migrationBuilder.DropTable(
                name: "Timelines");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
