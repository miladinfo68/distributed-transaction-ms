using Microsoft.EntityFrameworkCore.Migrations;

namespace SagaStateMachine.Service.Migrations
{
    public partial class addorderdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "OrderStateInstance",
                newName: "OrderDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "OrderStateInstance",
                newName: "CreatedDate");
        }
    }
}
