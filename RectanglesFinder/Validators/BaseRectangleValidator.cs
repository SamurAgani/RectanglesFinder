using FluentValidation;
using RectanglesFinder.Models;

namespace RectanglesFinder.Validators
{
    public class BaseRectangleValidator : AbstractValidator<BaseRectangle>
    {
        public BaseRectangleValidator()
        {
            RuleFor(x => x.Xmin)
                .LessThan(x => x.Xmax)
                .WithMessage("Xmin cannot be greater or equal to Xmax.");

            RuleFor(x => x.Ymin)
                .LessThan(x => x.Ymax)
                .WithMessage("Ymin cannot be greater or equal to Ymax.");
        }
    }
}
