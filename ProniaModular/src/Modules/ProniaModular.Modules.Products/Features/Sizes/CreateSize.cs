using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record CreateSizeCommand(string Name) : ICommand<Result<SizeDto>>;

    public sealed class CreateSizeCommandValidator : AbstractValidator<CreateSizeCommand>
    {
        public CreateSizeCommandValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Size name is required.")
                .MaximumLength(50).WithMessage("Size name must not exceed 50 characters.");
        }
    }

    public sealed class CreateSizeCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<CreateSizeCommand, Result<SizeDto>>
    {
        public async Task<Result<SizeDto>> Handle(CreateSizeCommand command, CancellationToken cancellationToken = default)
        {
            var nameExists = await dbContext.Sizes
                .AnyAsync(s => s.Name == command.Name, cancellationToken);

            if (nameExists)
                return Result<SizeDto>.Failure(Error.Conflict($"A size named '{command.Name}' already exists."));

            var size = new Size
            {
                Name = command.Name,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Sizes.Add(size);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<SizeDto>.Success(new SizeDto(size.Id, size.Name));
        }
    }
}
