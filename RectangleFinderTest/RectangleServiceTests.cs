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
            var searchRectangle = new BaseRectangle
            {
                Xmin = 0,
                Xmax = 50,
                Ymin = 0,
                Ymax = 50
            };

            // Simulate a list of rectangles to return from the repository
            var allRectangles = new List<Rectangle>
            {
                new Rectangle { Id = 1, Xmin = 10, Xmax = 40, Ymin = 10, Ymax = 40 },
                new Rectangle { Id = 2, Xmin = 40, Xmax = 80, Ymin = 40, Ymax = 80 },
                new Rectangle { Id = 3, Xmin = -10, Xmax = 20, Ymin = -1, Ymax = 20 }
            };

            // Setup the repository to return a successful response
            _rectangleRepositoryMock.Setup(repo => repo.GetAll())
                .ReturnsAsync(BaseResponse<IEnumerable<Rectangle>>.Success(allRectangles));

            // Act
            var response = await _rectangleService.SearchRectangles(searchRectangle);

            // Assert
            Assert.IsTrue(response.IsSuccessful);
            var result = response.Data.ToList();
            Assert.AreEqual(2, result.Count); // Only two rectangles match the search criteria
            Assert.AreEqual(2, result[0].Id);
            Assert.AreEqual(3, result[1].Id);
        }

        [Test]
        public async Task Validate_ShouldFail_WhenXminGreaterThanXmax()
        {
            // Arrange
            var invalidRectangle = new BaseRectangle { Xmax = 0, Xmin = 100, Ymax = 100, Ymin = 0 };

            // Act
            var result = await _rectangleService.Validate(invalidRectangle);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
        }

        [Test]
        public async Task Validate_ShouldFail_WhenYminGreaterThanYmax()
        {
            // Arrange
            var invalidRectangle = new BaseRectangle { Xmax = 100, Xmin = 0, Ymax = 0, Ymin = 100 };

            // Act
            var result = await _rectangleService.Validate(invalidRectangle);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
        }

        [Test]
        public async Task Validate_ShouldPass_WhenValid()
        {
            // Arrange
            var validRectangle = new BaseRectangle { Xmax = 100, Xmin = 0, Ymax = 100, Ymin = 0 };

            // Act
            var result = await _rectangleService.Validate(validRectangle);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
        }
    }
}