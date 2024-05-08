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

        var allRectangles = await _rectangleRepository.SearchIntersect(searchSegment);
        return allRectangles;
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
                Points = new List<Point>()
                    {
                        new Point { X = 3, Y = 3 },
                        new Point { X = 2, Y = 2 },
                        new Point { X = 4, Y = 2 },
                        new Point { X = 3, Y = 1 }

                    }
            });

            rectangles.Add(new BaseRectangle()
            {
                Points = new List<Point>()
                    {
                        new Point { X = 0, Y = 1 },
                        new Point { X = 1, Y = 0 },
                        new Point { X = 0, Y = -1 },
                        new Point { X = -1, Y = 0 }

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

    private List<Point> OrderRectanglePoints(List<Point> points)
    {
        // Find the point with the smallest x (if tie, the smallest y)
        var ordered = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

        // Check which is the top-left point (smallest x and y, or smallest x and then smallest y if there's a tie)
        Point topLeft = ordered[0].Y < ordered[1].Y ? ordered[0] : ordered[1];
        Point bottomRight = ordered[2].Y > ordered[3].Y ? ordered[2] : ordered[3];

        // Top-right and bottom-left are the other two points
        Point topRight = ordered[0] == topLeft || ordered[1] == topLeft ? ordered[2] : ordered[0];
        Point bottomLeft = ordered[3] == bottomRight || ordered[2] == bottomRight ? ordered[1] : ordered[3];

        return new List<Point> { topLeft, topRight, bottomRight, bottomLeft };
    }
    private BaseRectangle CreateRectangle(int x1, int y1, int x2, int y2)
    {
        return new BaseRectangle
        {
            Points = new List<Point>
            {
                new Point { X = x1, Y = y1 },
                new Point { X = x2, Y = y1 },
                new Point { X = x2, Y = y2 },
                new Point { X = x1, Y = y2 }
            }
        };
    }


    #endregion
}
