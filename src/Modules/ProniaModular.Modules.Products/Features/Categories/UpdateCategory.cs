using FluentValidation;

namespace ProniaModular.Modules.Products.Features.Categories
{
    public sealed record UpdateCategoryCommand(long Id, string Name);

    public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(c => c.Id)
                .GreaterThan(0).WithMessage("A valid category id is required.");

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(200).WithMessage("Category name must not exceed 200 characters.");
        }
    }
}
