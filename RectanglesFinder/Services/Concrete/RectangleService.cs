// Services/RectangleService.cs
using Dapper;
using RectanglesFinder.Models;
using RectanglesFinder.Services.Interfaces;
using RectanglesFinder;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
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
        var validateResult = await Validate(rectangle);
        if (!validateResult.IsSuccessful)
            return BaseResponse<bool>.Fail(false, validateResult.Errors);

        if (rectangle.Id is null)
            return BaseResponse<bool>.Fail(false, "Id is required for update!");

        return await _rectangleRepository.UpdateRectangle(rectangle);
    }

    public async Task<BaseResponse<Rectangle>> GetById(int id)
    {
        return await _rectangleRepository.GetById(id);
    }

    public async Task<BaseResponse<IEnumerable<Rectangle>>> SearchRectangles(BaseRectangle searchRectangle)
    {
        var validateResult = await Validate(searchRectangle);
        if (!validateResult.IsSuccessful)
            return BaseResponse<IEnumerable<Rectangle>>.Fail(null, validateResult.Errors);

        var allRectangles = await _rectangleRepository.GetAll();
        var rectangles = allRectangles.Data.Where(r => !(r.Xmax < searchRectangle.Xmin || r.Xmin > searchRectangle.Xmax ||
                                                           r.Ymax < searchRectangle.Ymin || r.Ymin > searchRectangle.Ymax))
                                             .ToList();
        return BaseResponse<IEnumerable<Rectangle>>.Success(rectangles);
    }


    public async Task SeedRectangles(int count)
    {
        var existingCount = (await _rectangleRepository.GetAll()).Data.Count();

        if (existingCount == 0)
        {
            for (int i = 1; i <= count; i++)
            {
                var rect = new BaseRectangle
                {
                    Xmax = i * 10,
                    Xmin = (i - 1) * 10,
                    Ymax = i * 10,
                    Ymin = (i - 1) * 10
                };
                await _rectangleRepository.AddRectangle(rect);
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
}
