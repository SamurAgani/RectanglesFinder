using Microsoft.AspNetCore.Http;
namespace RectanglesFinder.Models
{
    public class BaseRectangle
    {
        public int Xmax { get; set; }
        public int Xmin { get; set; }
        public int Ymax { get; set; }
        public int Ymin { get; set; }
    }
}
