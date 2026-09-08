using FluentValidation;

namespace ProniaModular.Modules.Products.Features.Categories
{
    public sealed record CreateCategoryCommand(string Name);

    public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(200).WithMessage("Category name must not exceed 200 characters.");
        }
    }
}
