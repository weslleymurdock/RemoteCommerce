namespace RemoteCommerce.Application.Catalog.Validators;

/// <summary>Validates product creation commands.</summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>Initializes product creation rules.</summary>
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
        RuleFor(x => x.Request.Sku).MaximumLength(100).When(x => x.Request.Sku is not null);
        RuleFor(x => x.Request.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.Request.CompareAtPrice.HasValue);
        RuleFor(x => x.Request.Currency).Length(3);
    }
}

/// <summary>Validates product update commands.</summary>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>Initializes product update rules.</summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
        RuleFor(x => x.Request.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.Request.CompareAtPrice.HasValue);
        RuleFor(x => x.Request.Currency).Length(3);
    }
}

/// <summary>Validates category creation commands.</summary>
public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    /// <summary>Initializes category creation rules.</summary>
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
        RuleFor(x => x.Request.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

/// <summary>Validates category update commands.</summary>
public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    /// <summary>Initializes category update rules.</summary>
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
        RuleFor(x => x.Request.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

/// <summary>Validates brand creation commands.</summary>
public sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    /// <summary>Initializes brand creation rules.</summary>
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
    }
}

/// <summary>Validates brand update commands.</summary>
public sealed class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    /// <summary>Initializes brand update rules.</summary>
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
    }
}

/// <summary>Validates tag creation commands.</summary>
public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    /// <summary>Initializes tag creation rules.</summary>
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
    }
}

/// <summary>Validates tag update commands.</summary>
public sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    /// <summary>Initializes tag update rules.</summary>
    public UpdateTagCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
    }
}

/// <summary>Validates product variation creation commands.</summary>
public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    /// <summary>Initializes variation rules.</summary>
    public CreateProductVariantCommandValidator()
    {
        RuleFor(x => x.Request.ProductId).NotEmpty();
        RuleFor(x => x.Request.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.Request.CompareAtPrice.HasValue);
        RuleFor(x => x.Request.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

/// <summary>Validates product variation update commands.</summary>
public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    /// <summary>Initializes variation update rules.</summary>
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.Request.ProductId).NotEmpty();
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

/// <summary>Validates product metadata commands.</summary>
public sealed class ProductMetadataValidator : AbstractValidator<UpsertProductMetadataCommand>
{
    /// <summary>Initializes metadata rules and rejects secret-like keys.</summary>
    public ProductMetadataValidator()
    {
        RuleFor(x => x.Request.ProductId).NotEmpty();
        RuleFor(x => x.Request.Key)
            .NotEmpty()
            .MaximumLength(200)
            .Must(x => !x.Contains("password", StringComparison.OrdinalIgnoreCase))
            .Must(x => !x.Contains("secret", StringComparison.OrdinalIgnoreCase))
            .Must(x => !x.Contains("token", StringComparison.OrdinalIgnoreCase))
            .Must(x => !x.Contains("connectionstring", StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.Request.Value).NotEmpty().MaximumLength(10000);
    }
}
