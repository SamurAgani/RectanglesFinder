using RectanglesFinder.Models;
using System.Collections.Generic;

namespace RectanglesFinder.Services.Interfaces
{
    public interface IRectangleService
    {
        Task<BaseResponse<IEnumerable<Rectangle>>> GetAll();
        Task<BaseResponse<IEnumerable<Rectangle>>> SearchRectangles(BaseRectangle searchRectangle);
        Task<BaseResponse<Rectangle>> GetById(int id);
        Task<BaseResponse<bool>> AddRectangle(BaseRectangle rectangle);
        Task<BaseResponse<bool>> UpdateRectangle(Rectangle rectangle);
        Task<BaseResponse<bool>> DeleteRectangle(int id);
        Task SeedRectangles(int count);
        Task<BaseResponse<bool>> Validate(BaseRectangle baseRectangle);
    }
}
