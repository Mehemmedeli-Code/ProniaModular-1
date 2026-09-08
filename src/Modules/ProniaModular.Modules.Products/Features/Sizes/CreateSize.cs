using FluentValidation;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record CreateSizeCommand(string Name);

    public sealed class CreateSizeCommandValidator : AbstractValidator<CreateSizeCommand>
    {
        public CreateSizeCommandValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Size name is required.")
                .MaximumLength(50).WithMessage("Size name must not exceed 50 characters.");
        }
    }
}
