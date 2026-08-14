namespace RemoteCommerce.Application.Catalog.Validators;


/// <summary>Validates product creation.</summary>
public sealed class ProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>Initializes product rules.</summary>
    public ProductCommandValidator()
    {
        RuleFor(x => x.data.Name).NotEmpty().MaximumLength(200); 
        RuleFor(x => x.data.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); 
        RuleFor(x => x.data.Sku).MaximumLength(100).When(x => x.data.Sku is not null); 
        RuleFor(x => x.data.Price).GreaterThanOrEqualTo(0); 
        RuleFor(x => x.data.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.data.CompareAtPrice.HasValue); 
        RuleFor(x => x.data.Currency).Length(3);
    }
}
/// <summary>Validates product updates.</summary>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>Initializes product update rules.</summary>
    public UpdateProductCommandValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.CompareAtPrice.HasValue); RuleFor(x => x.Currency).Length(3); }
}
/// <summary>Validates category creation.</summary>
public sealed class CategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    /// <summary>Initializes category creation rules.</summary>
    public CategoryCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0); }
}
/// <summary>Validates category updates.</summary>
public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    /// <summary>Initializes category update rules.</summary>
    public UpdateCategoryCommandValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0); }
}
/// <summary>Validates brand creation.</summary>
public sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    /// <summary>Initializes brand creation rules.</summary>
    public CreateBrandCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}
/// <summary>Validates brand updates.</summary>
public sealed class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    /// <summary>Initializes brand update rules.</summary>
    public UpdateBrandCommandValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}
/// <summary>Validates tag creation.</summary>
public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    /// <summary>Initializes tag creation rules.</summary>
    public CreateTagCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}
/// <summary>Validates tag updates.</summary>
public sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    /// <summary>Initializes tag update rules.</summary>
    public UpdateTagCommandValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}
/// <summary>Validates variant creation.</summary>
public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    /// <summary>Initializes variant rules.</summary>
    public CreateProductVariantCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.Sku).NotEmpty().MaximumLength(100); RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.CompareAtPrice.HasValue); RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0); }
}
/// <summary>Validates variant updates.</summary>
public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    /// <summary>Initializes variant update rules.</summary>
    public UpdateProductVariantCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Sku).NotEmpty().MaximumLength(100); RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0); }
}
/// <summary>Validates product metadata.</summary>
public sealed class ProductMetadataValidator : AbstractValidator<UpsertProductMetadataCommand>
{
    /// <summary>Initializes metadata rules and rejects secret-like keys.</summary>
    public ProductMetadataValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.Key).NotEmpty().MaximumLength(200).Must(x => !x.Contains("password", StringComparison.OrdinalIgnoreCase) && !x.Contains("secret", StringComparison.OrdinalIgnoreCase) && !x.Contains("token", StringComparison.OrdinalIgnoreCase) && !x.Contains("connectionstring", StringComparison.OrdinalIgnoreCase)); RuleFor(x => x.Value).NotEmpty().MaximumLength(10000); }
}
