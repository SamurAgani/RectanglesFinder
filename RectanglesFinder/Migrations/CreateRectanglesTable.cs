using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RectanglesFinder.Migrations
{
    [Migration(1)]
    public class CreateRectanglesTable : Migration
    {
        public override void Up()
        {
            Create.Table("Rectangle")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Xmin").AsInt32().NotNullable()
                .WithColumn("Ymin").AsInt32().NotNullable()
                .WithColumn("Xmax").AsInt32().NotNullable()
                .WithColumn("Ymax").AsInt32().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("Rectangle");
        }
    }
}
