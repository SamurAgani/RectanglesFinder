using RectanglesFinder.Models;

namespace RectanglesFinder.Repositories
{
    public interface IRectangleRepository
    {
        Task<BaseResponse<bool>> AddRectangle(BaseRectangle rectangle);
        Task<BaseResponse<bool>> DeleteRectangle(int id);
        Task<BaseResponse<IEnumerable<Rectangle>>> GetAll();
        Task<BaseResponse<bool>> UpdateRectangle(Rectangle rectangle);
        Task<BaseResponse<Rectangle>> GetById(int id);
    }
}
