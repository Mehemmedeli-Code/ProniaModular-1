using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Products
{
    public sealed record DeleteProductCommand(long Id) : ICommand<Result<bool>>;

    public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(p => p.Id)
                .GreaterThan(0).WithMessage("A valid product id is required.");
        }
    }

    public sealed class DeleteProductCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<DeleteProductCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteProductCommand command, CancellationToken cancellationToken = default)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);
            if (product is null)
                return Result<bool>.Failure(Error.NotFound($"Product with id {command.Id} was not found."));

            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
