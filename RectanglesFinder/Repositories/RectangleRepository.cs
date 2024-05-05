using Dapper;
using RectanglesFinder.Models;
using System.Data.SqlClient;

namespace RectanglesFinder.Repositories
{
    public class RectangleRepository : IRectangleRepository
    {
        private readonly string _connectionString;

        public RectangleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<BaseResponse<bool>> AddRectangle(BaseRectangle rectangle)
        {
            using var connection = GetConnection();
            var query = "INSERT INTO Rectangle (Xmax, Xmin, Ymax, Ymin) VALUES (@Xmax, @Xmin, @Ymax, @Ymin)";
            var result = await connection.ExecuteAsync(query, rectangle);
            return BaseResponse<bool>.Success(result > 0);
        }

        public async Task<BaseResponse<bool>> DeleteRectangle(int id)
        {
            using var connection = GetConnection();
            var query = "DELETE FROM Rectangle WHERE Id = @Id";
            var result = await connection.ExecuteAsync(query, new { Id = id });
            return BaseResponse<bool>.Success(result > 0);
        }

        public async Task<BaseResponse<IEnumerable<Rectangle>>> GetAll()
        {
            using var connection = GetConnection();
            var query = "SELECT * FROM Rectangle";
            var rectangles = await connection.QueryAsync<Rectangle>(query);
            return BaseResponse<IEnumerable<Rectangle>>.Success(rectangles);
        }

        public async Task<BaseResponse<bool>> UpdateRectangle(Rectangle rectangle)
        {
            using var connection = GetConnection();
            var query = "UPDATE Rectangle SET Xmax = @Xmax, Xmin = @Xmin, Ymax = @Ymax, Ymin = @Ymin WHERE Id = @Id";
            var result = await connection.ExecuteAsync(query, rectangle);
            return BaseResponse<bool>.Success(result > 0);
        }

        public async Task<BaseResponse<Rectangle>> GetById(int id)
        {
            using var connection = GetConnection();
            var query = "SELECT * FROM Rectangle WHERE Id = @Id";
            var rectangle = await connection.QueryFirstOrDefaultAsync<Rectangle>(query, new { Id = id });
            return BaseResponse<Rectangle>.Success(rectangle);
        }
    }
}

