using Dapper;
using RectanglesFinder.Models;
using System.Collections.Generic;
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

            var insertRectangleSql = @"
            INSERT INTO Rectangle
            DEFAULT VALUES;
            SELECT CAST(SCOPE_IDENTITY() as int);";

            var rectangleId = await connection.QuerySingleAsync<int>(insertRectangleSql);

            foreach (var point in rectangle.Points)
            {
                var insertPointSql = @"
                INSERT INTO Point (X, Y, RectangleId)
                VALUES (@X, @Y, @RectangleId);";
                await connection.ExecuteAsync(insertPointSql, new { X = point.X, Y = point.Y, RectangleId = rectangleId });
            }


            return BaseResponse<bool>.Success(true);
        }

        public async Task<BaseResponse<bool>> DeleteRectangle(int id)
        {
            using var connection = GetConnection();

            await connection.ExecuteAsync("DELETE FROM Point WHERE RectangleId = @Id", new { Id = id });
            await connection.ExecuteAsync("DELETE FROM Rectangle WHERE Id = @Id", new { Id = id });

            return BaseResponse<bool>.Success(true);

        }


        public async Task<BaseResponse<IEnumerable<Rectangle>>> GetAll()
        {
            using var connection = GetConnection();

            var rectangleDictionary = new Dictionary<int, Rectangle>();

            var rectangles = await connection.QueryAsync<Rectangle, BasePoint, Rectangle>(
                "SELECT r.*, p.* FROM Rectangle r INNER JOIN Point p ON r.Id = p.RectangleId",
                (rectangle, point) =>
                {
                    if (!rectangleDictionary.TryGetValue(rectangle.Id.Value, out var rectangleEntry))
                    {
                        rectangleEntry = rectangle;
                        rectangleEntry.Points = new List<BasePoint>();
                        rectangleDictionary.Add(rectangleEntry.Id.Value, rectangleEntry);
                    }

                    rectangleEntry.Points.Add(point);
                    return rectangleEntry;
                },
                splitOn: "Id");


            return BaseResponse<IEnumerable<Rectangle>>.Success(rectangles.Distinct());
        }


        public async Task<BaseResponse<bool>> UpdateRectangle(Rectangle rectangle)
        {
            using var connection = GetConnection();
            foreach (var point in rectangle.Points)
            {
                var updatePointSql = @"
                UPDATE Point
                SET X = @X, Y = @Y
                WHERE Id = @Id AND RectangleId = @RectangleId";

                await connection.ExecuteAsync(updatePointSql, new { X = point.X, Y = point.Y, Id = point.Id, RectangleId = rectangle.Id });
            }
            return BaseResponse<bool>.Success(true);

        }

        public async Task<BaseResponse<Rectangle>> GetById(int id)
        {
            using var connection = GetConnection();

            var result = await GetAll();
            var rectangle = result.Data.FirstOrDefault(r => r.Id == id);

            return BaseResponse<Rectangle>.Success(rectangle);
        }
    }
}

