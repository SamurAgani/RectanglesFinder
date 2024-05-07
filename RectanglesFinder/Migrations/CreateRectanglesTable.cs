using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RectanglesFinder.Migrations
{
    [Migration(2)]
    public class CreateRectanglesTable : Migration
    {
        public override void Up()
        {


            Create.Table("Rectangle")
             .WithColumn("Id").AsInt32().PrimaryKey().Identity();


            Create.Table("Point")
             .WithColumn("Id").AsInt32().PrimaryKey().Identity()
             .WithColumn("X").AsInt32()
             .WithColumn("Y").AsInt32()
             .WithColumn("RectangleId").AsInt32().ForeignKey("FK_BaseRectangles_Id", "Rectangle", "Id");

        }

        public override void Down()
        {
            Delete.Table("Rectangle");
            Delete.Table("Points");
        }
    }
}
