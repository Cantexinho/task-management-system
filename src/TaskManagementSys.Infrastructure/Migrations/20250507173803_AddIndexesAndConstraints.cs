using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementSys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                table: "TaskAssignments",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Comments",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedByUserId",
                table: "Tasks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Priority",
                table: "Tasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status",
                table: "Tasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status_DueDate",
                table: "Tasks",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaskItem_CompletedAt",
                table: "Tasks",
                sql: "Status != 3 OR CompletedAt IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_AssignedById",
                table: "TaskAssignments",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_IsActive",
                table: "TaskAssignments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_UserId",
                table: "TaskAssignments",
                column: "UserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaskAssignment_DeactivatedAt",
                table: "TaskAssignments",
                sql: "IsActive = 1 OR DeactivatedAt IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedByUserId",
                table: "Projects",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Project_EndDate_After_StartDate",
                table: "Projects",
                sql: "EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedAt",
                table: "Comments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Comment_Content_NotEmpty",
                table: "Comments",
                sql: "LENGTH(TRIM(Content)) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Category_Color_Format",
                table: "Categories",
                sql: "Color IS NULL OR Color LIKE '#%' AND LENGTH(Color) <= 9");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Category_Name_NotEmpty",
                table: "Categories",
                sql: "LENGTH(TRIM(Name)) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedByUserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Priority",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Status",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Status_DueDate",
                table: "Tasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TaskItem_CompletedAt",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_AssignedById",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_IsActive",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_UserId",
                table: "TaskAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TaskAssignment_DeactivatedAt",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedByUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Status",
                table: "Projects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Project_EndDate_After_StartDate",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Comments_CreatedAt",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId",
                table: "Comments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Comment_Content_NotEmpty",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Category_Color_Format",
                table: "Categories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Category_Name_NotEmpty",
                table: "Categories");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                table: "TaskAssignments",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Comments",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
