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
             .WithColumn("X").AsDouble()
             .WithColumn("Y").AsDouble()
             .WithColumn("RectangleId").AsInt32().ForeignKey("FK_BaseRectangles_Id", "Rectangle", "Id");

            Execute.Sql(@"CREATE FUNCTION dbo.Direction
                        (
                            @piX FLOAT, @piY FLOAT,
                            @pjX FLOAT, @pjY FLOAT,
                            @pkX FLOAT, @pkY FLOAT
                        )
                        RETURNS FLOAT
                        AS
                        BEGIN
                            RETURN (@pkX - @piX) * (@pjY - @piY) - (@pjX - @piX) * (@pkY - @piY);
                        END");
            Execute.Sql(@"CREATE FUNCTION dbo.OnSegment
                        (
                            @piX FLOAT, @piY FLOAT,
                            @pjX FLOAT, @pjY FLOAT,
                            @pkX FLOAT, @pkY FLOAT
                        )
                        RETURNS BIT
                        AS
                        BEGIN
                            IF (@pkX BETWEEN CASE WHEN @piX < @pjX THEN @piX ELSE @pjX END AND CASE WHEN @piX > @pjX THEN @piX ELSE @pjX END AND
                                @pkY BETWEEN CASE WHEN @piY < @pjY THEN @piY ELSE @pjY END AND CASE WHEN @piY > @pjY THEN @piY ELSE @pjY END)
                                RETURN 1;  -- True
                            
                            RETURN 0;  -- False
                        END");

            Execute.Sql(@"CREATE FUNCTION dbo.Intersects
                        (
                            @p1X FLOAT, @p1Y FLOAT, @p2X FLOAT, @p2Y FLOAT,
                            @p3X FLOAT, @p3Y FLOAT, @p4X FLOAT, @p4Y FLOAT
                        )
                        RETURNS BIT
                        AS
                        BEGIN
                            DECLARE @d1 FLOAT, @d2 FLOAT, @d3 FLOAT, @d4 FLOAT;
                        
                            -- Calculate directions
                            SET @d1 = dbo.Direction(@p3X, @p3Y, @p4X, @p4Y, @p1X, @p1Y);
                            SET @d2 = dbo.Direction(@p3X, @p3Y, @p4X, @p4Y, @p2X, @p2Y);
                            SET @d3 = dbo.Direction(@p1X, @p1Y, @p2X, @p2Y, @p3X, @p3Y);
                            SET @d4 = dbo.Direction(@p1X, @p1Y, @p2X, @p2Y, @p4X, @p4Y);
                        
                            -- Check if the segments straddle each other
                            IF ((@d1 > 0 AND @d2 < 0 OR @d1 < 0 AND @d2 > 0) AND
                                (@d3 > 0 AND @d4 < 0 OR @d3 < 0 AND @d4 > 0))
                                RETURN 1;  -- True
                        
                            -- Check for collinearity
                            IF (@d1 = 0 AND dbo.OnSegment(@p3X, @p3Y, @p4X, @p4Y, @p1X, @p1Y) = 1 OR
                                @d2 = 0 AND dbo.OnSegment(@p3X, @p3Y, @p4X, @p4Y, @p2X, @p2Y) = 1 OR
                                @d3 = 0 AND dbo.OnSegment(@p1X, @p1Y, @p2X, @p2Y, @p3X, @p3Y) = 1 OR
                                @d4 = 0 AND dbo.OnSegment(@p1X, @p1Y, @p2X, @p2Y, @p4X, @p4Y) = 1)
                                RETURN 1;  -- True
                        
                            RETURN 0;  -- False
                        END");


            Execute.Sql(@"CREATE OR ALTER PROCEDURE CheckAllRectanglesIntersection
                            @segmentStartX FLOAT,
                            @segmentStartY FLOAT,
                            @segmentEndX FLOAT,
                            @segmentEndY FLOAT
                        AS
                        BEGIN
                            -- Ensure any existing cursor is closed and deallocated
                            IF CURSOR_STATUS('global', 'cur') >= 0
                            BEGIN
                                CLOSE cur;
                                DEALLOCATE cur;
                            END
                        
                            -- Temporary table to store intersecting rectangles
                            CREATE TABLE #IntersectingRectangles
                            (
                                RectangleId INT
                            );
                        
                            DECLARE @rectangleId INT;
                            DECLARE cur CURSOR FOR 
                                SELECT Id FROM Rectangle;
                        
                            OPEN cur;
                        
                            FETCH NEXT FROM cur INTO @rectangleId;
                        
                            WHILE @@FETCH_STATUS = 0
                            BEGIN
                                DECLARE @p1X FLOAT, @p1Y FLOAT, @p2X FLOAT, @p2Y FLOAT,
                                        @p3X FLOAT, @p3Y FLOAT, @p4X FLOAT, @p4Y FLOAT;
                        
                                -- Fetch points using a CTE within the same statement to avoid scope issues
                                ;WITH OrderedPoints AS (
                                    SELECT X, Y, ROW_NUMBER() OVER (ORDER BY ID ASC) AS PointOrder
                                    FROM Point WHERE RectangleId = @rectangleId
                                )
                                SELECT
                                    @p1X = MAX(CASE WHEN PointOrder = 1 THEN X ELSE NULL END),
                                    @p1Y = MAX(CASE WHEN PointOrder = 1 THEN Y ELSE NULL END),
                                    @p2X = MAX(CASE WHEN PointOrder = 2 THEN X ELSE NULL END),
                                    @p2Y = MAX(CASE WHEN PointOrder = 2 THEN Y ELSE NULL END),
                                    @p3X = MAX(CASE WHEN PointOrder = 3 THEN X ELSE NULL END),
                                    @p3Y = MAX(CASE WHEN PointOrder = 3 THEN Y ELSE NULL END),
                                    @p4X = MAX(CASE WHEN PointOrder = 4 THEN X ELSE NULL END),
                                    @p4Y = MAX(CASE WHEN PointOrder = 4 THEN Y ELSE NULL END)
                                FROM OrderedPoints;
                        
                                -- Check intersection for each segment of the rectangle
                                IF (dbo.Intersects(@segmentStartX, @segmentStartY, @segmentEndX, @segmentEndY, @p1X, @p1Y, @p2X, @p2Y) = 1 OR
                                    dbo.Intersects(@segmentStartX, @segmentStartY, @segmentEndX, @segmentEndY, @p2X, @p2Y, @p3X, @p3Y) = 1 OR
                                    dbo.Intersects(@segmentStartX, @segmentStartY, @segmentEndX, @segmentEndY, @p3X, @p3Y, @p4X, @p4Y) = 1 OR
                                    dbo.Intersects(@segmentStartX, @segmentStartY, @segmentEndX, @segmentEndY, @p4X, @p4Y, @p1X, @p1Y) = 1)
                                BEGIN
                                    INSERT INTO #IntersectingRectangles (RectangleId)
                                    VALUES (@rectangleId);
                                END;
                        
                                FETCH NEXT FROM cur INTO @rectangleId;
                            END;
                        
                            CLOSE cur;
                            DEALLOCATE cur;
                        
                            -- Return all intersecting rectangles
                            SELECT r.*, p.* FROM Rectangle r INNER JOIN Point p ON r.Id = p.RectangleId
                            INNER JOIN #IntersectingRectangles ir ON r.Id = ir.RectangleId;
                        
                            DROP TABLE #IntersectingRectangles;
                        END;");

        }

        public override void Down()
        {
            Delete.Table("Rectangle");
            Delete.Table("Points");
        }
    }
}
