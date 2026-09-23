// .../CreateCategoryCommandHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Application.Commands.CreateCategoryCommand;

public sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<CreateCategoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Name);
        await categoryRepository.AddAsync(category, cancellationToken);
        return Result<int>.Success(category.Id);
    }
}