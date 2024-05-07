using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RectanglesFinder.Models;
using RectanglesFinder.Services.Interfaces;

namespace RectanglesFinder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RectangleController : CustomBaseController
    {

        private readonly IRectangleService _rectangleService;

        public RectangleController(IRectangleService rectangleService)
        {
            _rectangleService = rectangleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRectangles()
        {
            var rectangles = await _rectangleService.GetAll();
            return CreateActionResultInstance(rectangles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRectangle(int id)
        {
            var rectangle = await _rectangleService.GetById(id);
            return CreateActionResultInstance(rectangle);
        }

        [HttpPost]
        public async Task<IActionResult> AddRectangle([FromBody] BaseRectangle rectangle)
        {
            var response = await _rectangleService.AddRectangle(rectangle);
            return CreateActionResultInstance(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRectangle([FromBody] Rectangle rectangle)
        {
            var response = await _rectangleService.UpdateRectangle(rectangle);
            return CreateActionResultInstance(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRectangle(int id)
        {
            var response = await _rectangleService.DeleteRectangle(id);
            return CreateActionResultInstance(response);
        }

        [HttpPost("SearchRectangle")]
        public async Task<IActionResult> SearchRectangle([FromBody] SearchSegment segmentSearch)
        {
            var response = await _rectangleService.SearchRectangles(segmentSearch);
            return CreateActionResultInstance(response);
        }
    }
}
