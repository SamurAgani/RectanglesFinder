using Dapper;
using Moq;
using RectanglesFinder;
using RectanglesFinder.Models;
using RectanglesFinder.Repositories;
using RectanglesFinder.Services.Interfaces;
using System.Data;

namespace RectangleFinderTest
{
    [TestFixture]
    public class RectangleServiceTests
    {
        private RectangleService _rectangleService;
        private Mock<IRectangleRepository> _rectangleRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _rectangleRepositoryMock = new Mock<IRectangleRepository>();
            _rectangleService = new RectangleService(_rectangleRepositoryMock.Object);
        }

        [Test]
        public async Task SearchRectangles_ReturnsExpectedRectangles()
        {
            // Arrange
            var searchRectangle = new SearchSegment
            {
                StartPoint = new BasePoint() { X = 11, Y = 0 },
                EndPoint = new BasePoint() { X = -11, Y = 0 }
            };

            // Simulate a list of rectangles to return from the repository
            var allRectangles = new List<Rectangle>
            {
                new Rectangle()
                {
                    Id = 2,
                    Points = new List<Point>()
                        {
                            new Point { X = 0, Y = 1 },
                            new Point { X = 1, Y = 0 },
                            new Point { X = 0, Y = -1 },
                            new Point { X = -1, Y = 0 }
               
                        }
                },
                new Rectangle()
                {
                    Id = 2,
                    Points = new List<Point>()
                        {
                            new Point { X = 3, Y = 3 },
                            new Point { X = 2, Y = 2 },
                            new Point { X = 4, Y = 2 },
                            new Point { X = 3, Y = 1 }
               
                        }
                },
                new Rectangle()
                {
                    Id = 3,
                    Points = new List<Point>()
                        {
                            new Point { X = 3, Y = 13 },
                            new Point { X = 12, Y = 2 },
                            new Point { X = 4, Y = 2 },
                            new Point { X = 3, Y = 111 }
               
                        }
                }
            };

            // Setup the repository to return a successful response
            _rectangleRepositoryMock.Setup(repo => repo.GetAll())
                .ReturnsAsync(BaseResponse<IEnumerable<Rectangle>>.Success(allRectangles));

            // Act
            var response = await _rectangleService.SearchRectangles(searchRectangle);

            // Assert
            Assert.IsTrue(response.IsSuccessful);
            var result = response.Data.ToList();
            Assert.AreEqual(1, result.Count); // Only two rectangles match the search criteria
            Assert.AreEqual(2, result[0].Id);
        }


        [Test]
        public async Task Validate_ShouldFail_When()
        {
            // Arrange
            var validRectangle = new BaseRectangle()
            {
                Points = new List<Point>()
                    {
                        new Point { X = -2, Y = 3 },
                        new Point { X = -2, Y = 1 },
                        new Point { X = -4, Y = 2 },
                        new Point { X = 0, Y = 2 }

                    }
            };
            // Act
            var result = await _rectangleService.Validate(validRectangle);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
        }

        [Test]
        public async Task Validate_ShouldPass_WhenValid()
        {
            // Arrange

            var validRectangle = new BaseRectangle()
            {
                Points = new List<Point>()
                    {
                        new Point { X = 0, Y = 1 },
                        new Point { X = 1, Y = 0 },
                        new Point { X = 0, Y = -1 },
                        new Point { X = -1, Y = 0 }

                    }
            };

            // Act
            var result = await _rectangleService.Validate(validRectangle);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
        }
    }
}