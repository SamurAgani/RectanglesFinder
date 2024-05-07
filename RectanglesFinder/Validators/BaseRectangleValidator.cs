using FluentValidation;
using RectanglesFinder.Models;

namespace RectanglesFinder.Validators
{
    public class BaseRectangleValidator : AbstractValidator<BaseRectangle>
    {
        public BaseRectangleValidator()
        {
            RuleFor(x => IsValidRectangle(x))
                .Equal(true)
                .WithMessage("Please add valid rectangle! Only 4 points allowed!");
        }
        public bool IsValidRectangle(BaseRectangle rectangle)
        {
            var points = rectangle.Points;
            if (points == null || points.Count != 4)
                return false;

            // Ensure points are in order: assuming p1-p2-p3-p4-p1
            bool rightAngles = IsRightAngle(points[0], points[1], points[2]) &&
                               IsRightAngle(points[1], points[2], points[3]) &&
                               IsRightAngle(points[2], points[3], points[0]) &&
                               IsRightAngle(points[3], points[0], points[1]);

            if (!rightAngles)
                return false;

            // Check opposite sides equality
            double side1 = Distance(points[0], points[1]);
            double side2 = Distance(points[1], points[2]);
            double side3 = Distance(points[2], points[3]);
            double side4 = Distance(points[3], points[0]);

            return side1 == side3 && side2 == side4;
        }
        private double Distance(BasePoint p1, BasePoint p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        private bool IsRightAngle(BasePoint p1, BasePoint p2, BasePoint p3)
        {
            // Vector from p1 to p2
            int dx1 = p2.X - p1.X;
            int dy1 = p2.Y - p1.Y;

            // Vector from p2 to p3
            int dx2 = p3.X - p2.X;
            int dy2 = p3.Y - p2.Y;

            // Check if the dot product is zero
            return dx1 * dx2 + dy1 * dy2 == 0;
        }
    }
}
