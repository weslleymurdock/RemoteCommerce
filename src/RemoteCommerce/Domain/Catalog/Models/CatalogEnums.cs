namespace RemoteCommerce.Domain.Catalog.Models;

/// <summary>Identifies the lifecycle state of a catalog product.</summary>
public enum ProductStatus 
{
    /// <summary>Product is in draft state and not visible to customers.</summary>
    Draft, 
    /// <summary>Product is published and visible to customers.</summary>
    Published,
    /// <summary>Product is archived and not visible to customers. </summary>
    Archived
}

/// <summary>Identifies the supported catalog product representation.</summary>
public enum ProductType 
{ 
    ///<summary>Represents a simple product.</summary>
    Simple,
    ///<summary>Represents a variable product.</summary>
    Variable, 
    ///<summary>Represents a virtual product.</summary>
    Virtual, 
    ///<summary>Represents a downloadable product.</summary>
    Downloadable 
}

/// <summary>Identifies the lifecycle state of a product variant.</summary>
public enum ProductVariantStatus 
{
    /// <summary>Product variant is in draft state and not visible to customers.</summary>
    Draft,
    /// <summary>Product variant is active and visible to customers.</summary>
    Active,
    /// <summary>Product variant is archived and not visible to customers.</summary>
    Archived
}

/// <summary>Identifies the storage role of media associated with a product.</summary>
public enum ProductMediaRole 
{
    /// <summary>Gallery image for the product.</summary>
    Gallery, 
    /// <summary>Thumbnail image for the product.</summary>
    Thumbnail 
}

/// <summary>Identifies the scalar representation of extensible metadata.</summary>
public enum MetadataValueType 
{
    /// <summary>Represents a string value.</summary>
    String, 
    /// <summary>Represents a number value.</summary>
    Number, 
    /// <summary>Represents a boolean value.</summary>
    Boolean,
    /// <summary>Represents a JSON value.</summary>
    Json
}
