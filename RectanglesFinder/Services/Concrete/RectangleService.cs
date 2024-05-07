using RectanglesFinder.Models;
using RectanglesFinder.Services.Interfaces;
using RectanglesFinder;
using RectanglesFinder.Validators;
using RectanglesFinder.Repositories;

public class RectangleService : IRectangleService
{
    private readonly IRectangleRepository _rectangleRepository;

    public RectangleService(IRectangleRepository rectangleRepository)
    {
        _rectangleRepository = rectangleRepository;
    }

    public async Task<BaseResponse<bool>> AddRectangle(BaseRectangle rectangle)
    {
        rectangle.Points = OrderRectanglePoints(rectangle.Points);
        var validateResult = await Validate(rectangle);
        if (!validateResult.IsSuccessful)
            return BaseResponse<bool>.Fail(false, validateResult.Errors);

        return await _rectangleRepository.AddRectangle(rectangle);
    }

    public async Task<BaseResponse<bool>> DeleteRectangle(int id)
    {
        return await _rectangleRepository.DeleteRectangle(id);
    }

    public async Task<BaseResponse<IEnumerable<Rectangle>>> GetAll()
    {
        return await _rectangleRepository.GetAll();
    }

    public async Task<BaseResponse<bool>> UpdateRectangle(Rectangle rectangle)
    {

        if (rectangle.Id is null)
            return BaseResponse<bool>.Fail(false, "Id is required for update!");


        if (rectangle.Points.Exists(x => x.Id == null))
            return BaseResponse<bool>.Fail(false, "Point Id is required for update!");

        rectangle.Points = OrderRectanglePoints(rectangle.Points);

        var validateResult = await Validate(rectangle);
        if (!validateResult.IsSuccessful)
            return BaseResponse<bool>.Fail(false, validateResult.Errors);

        return await _rectangleRepository.UpdateRectangle(rectangle);
    }

    public async Task<BaseResponse<Rectangle>> GetById(int id)
    {
        return await _rectangleRepository.GetById(id);
    }

    public async Task<BaseResponse<IEnumerable<Rectangle>>> SearchRectangles(SearchSegment searchSegment)
    {

        var allRectangles = await _rectangleRepository.GetAll();
        var rectangles = allRectangles.Data.Where(r => RectanglesIntersect(searchSegment.StartPoint, searchSegment.EndPoint, r))
                                           .ToList();
        return BaseResponse<IEnumerable<Rectangle>>.Success(rectangles);
    }

    public async Task SeedRectangles()
    {
        var existingCount = (await _rectangleRepository.GetAll()).Data.Count();

        if (existingCount == 0)
        {
            List<BaseRectangle> rectangles = new List<BaseRectangle>
                {
                    CreateRectangle(0, 0, 4, 2),
                    CreateRectangle(1, 1, 5, 3),
                    CreateRectangle(2, 2, 6, 5),
                    CreateRectangle(3, 3, 0, 6),
                    CreateRectangle(4, 4, 7, 8),
                    CreateRectangle(5, 5, 8, 9),
                    CreateRectangle(6, 6, 10, 10),
                    CreateRectangle(7, 7, 3, 11),
                    CreateRectangle(8, 8, 12, 12),
                    CreateRectangle(9, 9, 13, 13)
                };
            //rotated rectangles
            rectangles.Add(new BaseRectangle()
            {
                Points = new List<BasePoint>()
                    {
                        new BasePoint { X = 3, Y = 3 },
                        new BasePoint { X = 2, Y = 2 },
                        new BasePoint { X = 4, Y = 2 },
                        new BasePoint { X = 3, Y = 1 }

                    }
            });

            rectangles.Add(new BaseRectangle()
            {
                Points = new List<BasePoint>()
                    {
                        new BasePoint { X = 0, Y = 1 },
                        new BasePoint { X = 1, Y = 0 },
                        new BasePoint { X = 0, Y = -1 },
                        new BasePoint { X = -1, Y = 0 }

                    }
            });
            foreach (var rect in rectangles)
            {
                await AddRectangle(rect);
            }

        }
    }

    public async Task<BaseResponse<bool>> Validate(BaseRectangle baseRectangle)
    {
        var validator = new BaseRectangleValidator();
        var validationResult = await validator.ValidateAsync(baseRectangle);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            var response = BaseResponse<bool>.Fail(false, errors, 400);
            return response;
        }
        return BaseResponse<bool>.Success(true);
    }
    #region Private methods

    private List<BasePoint> OrderRectanglePoints(List<BasePoint> points)
    {
        // Find the point with the smallest x (if tie, the smallest y)
        var ordered = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

        // Check which is the top-left point (smallest x and y, or smallest x and then smallest y if there's a tie)
        BasePoint topLeft = ordered[0].Y < ordered[1].Y ? ordered[0] : ordered[1];
        BasePoint bottomRight = ordered[2].Y > ordered[3].Y ? ordered[2] : ordered[3];

        // Top-right and bottom-left are the other two points
        BasePoint topRight = ordered[0] == topLeft || ordered[1] == topLeft ? ordered[2] : ordered[0];
        BasePoint bottomLeft = ordered[3] == bottomRight || ordered[2] == bottomRight ? ordered[1] : ordered[3];

        return new List<BasePoint> { topLeft, topRight, bottomRight, bottomLeft };
    }
    private bool RectanglesIntersect(Point segmentStart, Point segmentEnd, Rectangle rectangle)
    {
        return (Intersects(segmentStart, segmentEnd, rectangle.Points[0], rectangle.Points[1]) ||
               Intersects(segmentStart, segmentEnd, rectangle.Points[1], rectangle.Points[2]) ||
               Intersects(segmentStart, segmentEnd, rectangle.Points[2], rectangle.Points[3]) ||
               Intersects(segmentStart, segmentEnd, rectangle.Points[3], rectangle.Points[0]));
    }

    public bool Intersects(Point p1, Point p2, Point p3, Point p4)
    {
        // Calculate direction of the points
        int d1 = Direction(p3, p4, p1);
        int d2 = Direction(p3, p4, p2);
        int d3 = Direction(p1, p2, p3);
        int d4 = Direction(p1, p2, p4);

        // Check if the segments straddle each other
        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) && ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        // Check for collinearity
        if (d1 == 0 && OnSegment(p3, p4, p1)) return true;
        if (d2 == 0 && OnSegment(p3, p4, p2)) return true;
        if (d3 == 0 && OnSegment(p1, p2, p3)) return true;
        if (d4 == 0 && OnSegment(p1, p2, p4)) return true;

        return false;
    }
    private int Direction(Point pi, Point pj, Point pk)
    {
        // Cross-product to find the direction
        return (pk.X - pi.X) * (pj.Y - pi.Y) - (pj.X - pi.X) * (pk.Y - pi.Y);
    }

    private bool OnSegment(Point pi, Point pj, Point pk)
    {
        if (Math.Min(pi.X, pj.X) <= pk.X && pk.X <= Math.Max(pi.X, pj.X) &&
            Math.Min(pi.Y, pj.Y) <= pk.Y && pk.Y <= Math.Max(pi.Y, pj.Y))
            return true;
        return false;
    }
    private BaseRectangle CreateRectangle(int x1, int y1, int x2, int y2)
    {
        return new BaseRectangle
        {
            Points = new List<BasePoint>
            {
                new BasePoint { X = x1, Y = y1 },
                new BasePoint { X = x2, Y = y1 },
                new BasePoint { X = x2, Y = y2 },
                new BasePoint { X = x1, Y = y2 }
            }
        };
    }


    #endregion
}
