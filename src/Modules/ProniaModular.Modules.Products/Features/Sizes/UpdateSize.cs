using FluentValidation;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record UpdateSizeCommand(long Id, string Name);

    public sealed class UpdateSizeCommandValidator : AbstractValidator<UpdateSizeCommand>
    {
        public UpdateSizeCommandValidator()
        {
            RuleFor(s => s.Id)
                .GreaterThan(0).WithMessage("A valid size id is required.");

            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Size name is required.")
                .MaximumLength(50).WithMessage("Size name must not exceed 50 characters.");
        }
    }
}
