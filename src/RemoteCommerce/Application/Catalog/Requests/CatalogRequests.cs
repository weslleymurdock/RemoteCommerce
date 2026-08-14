namespace RemoteCommerce.Application.Catalog.Requests;


/// <summary>
/// Represents a request to create a new product.
/// </summary>
/// <param name="name">Product Name.</param>
/// <param name="slug">Product Slug.</param>
/// <param name="sku">Product SKU.</param>
/// <param name="shortDescription">Product Short Description.</param>
/// <param name="description">Product Description.</param>
/// <param name="status">Product Status.</param>
/// <param name="productType">Product Type.</param>
/// <param name="price">Product Price.</param>
/// <param name="compareAtPrice">Product Compare At Price.</param>
/// <param name="currency">Product Currency.</param>
/// <param name="brandId">Product Brand ID.</param>
public sealed class CreateProductRequest(string name, string slug, string? sku, string shortDescription, string description, ProductStatus status, ProductType productType, decimal price, decimal? compareAtPrice, string currency, Guid? brandId)
{
    ///<summary>Product Name.</summary>
    public string Name { get; set; } = name;
    ///<summary>Product Slug.</summary>
    public string Slug {  get; set; } = slug;
    ///<summary>Product SKU.</summary>
    public string? Sku {  get; set; } = sku;
    ///<summary>Product Short Description.</summary>
    public string ShortDescription {  get; set; } = shortDescription;
    ///<summary>Product Description.</summary>
    public string Description {  get; set; } = description;
    ///<summary>Product Status.</summary>
    public ProductStatus Status {  get; set; } = status;
    ///<summary>Product Type.</summary>
    public ProductType ProductType {  get; set; } = productType;
    ///<summary>Product Price.</summary>
    public decimal Price {  get; set; } = price;
    ///<summary>Product Compare At Price.</summary>
    public decimal? CompareAtPrice {  get; set; } = compareAtPrice;
    ///<summary>Product Currency.</summary>
    public string Currency {  get; set; } = currency;
    ///<summary>Product Brand ID.</summary>
    public Guid? BrandId {  get; set; } = brandId;
}
