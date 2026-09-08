using FluentValidation;

namespace ProniaModular.Modules.Products.Features.Products
{
    public sealed record UpdateProductCommand(long Id, string Name, decimal Price, string? Description, long CategoryId);

    public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(p => p.Id)
                .GreaterThan(0).WithMessage("A valid product id is required.");

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.")
                .PrecisionScale(6, 2, false).WithMessage("Price must have at most 6 digits in total and 2 decimal places.");

            RuleFor(p => p.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(p => p.CategoryId)
                .GreaterThan(0).WithMessage("Product must belong to a valid category.");
        }
    }
}
