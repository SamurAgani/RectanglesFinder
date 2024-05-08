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