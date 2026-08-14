namespace RemoteCommerce.Domain.Catalog.Models;

/// <summary>Identifies the lifecycle state of a catalog product.</summary>
public enum ProductStatus { Draft, Published, Archived }

/// <summary>Identifies the supported catalog product representation.</summary>
public enum ProductType { Simple, Variable, Virtual, Downloadable }

/// <summary>Identifies the lifecycle state of a product variant.</summary>
public enum ProductVariantStatus { Draft, Active, Archived }

/// <summary>Identifies the storage role of media associated with a product.</summary>
public enum ProductMediaRole { Gallery, Thumbnail }

/// <summary>Identifies the scalar representation of extensible metadata.</summary>
public enum MetadataValueType { String, Number, Boolean, Json }
