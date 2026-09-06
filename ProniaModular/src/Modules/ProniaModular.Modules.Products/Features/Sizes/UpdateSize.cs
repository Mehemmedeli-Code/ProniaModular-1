using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record UpdateSizeCommand(long Id, string Name) : ICommand<Result<SizeDto>>;

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

    public sealed class UpdateSizeCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<UpdateSizeCommand, Result<SizeDto>>
    {
        public async Task<Result<SizeDto>> Handle(UpdateSizeCommand command, CancellationToken cancellationToken = default)
        {
            var size = await dbContext.Sizes.FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (size is null)
                return Result<SizeDto>.Failure(Error.NotFound($"Size with id {command.Id} was not found."));

            var nameTaken = await dbContext.Sizes
                .AnyAsync(s => s.Name == command.Name && s.Id != command.Id, cancellationToken);
            if (nameTaken)
                return Result<SizeDto>.Failure(Error.Conflict($"A size named '{command.Name}' already exists."));

            size.Name = command.Name;
            size.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<SizeDto>.Success(new SizeDto(size.Id, size.Name));
        }
    }
}
