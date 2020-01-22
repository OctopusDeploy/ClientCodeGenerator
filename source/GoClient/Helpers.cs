using System;

namespace Octopus.Server.CodeGen.Docs
{
    public static class Helpers
    {
        public static string FormatType(this string type, CollectionType collectionType)
        {
            if (type == PropertyDefinition.DateTimeOffsetType)
                type = "string";
            if (type == PropertyDefinition.LinkCollectionType)
                type = "Links";
            
            switch (collectionType)
            {
                case CollectionType.None:
                    return type;
                case CollectionType.List:
                    return "[]" + type;
                case CollectionType.ResourceCollection:
                    return type + "Collection";
                default:
                    throw new ArgumentOutOfRangeException(nameof(collectionType), collectionType, null);
            }
            
        }

        public static string GetPropertyMetadata(this PropertyDefinition property)
        {
            var result = $@"`json:""{property.Name},omitempty""";
            if (property.IsRequired)
                result += $@" validate:""required""";
            return result + "`";
        }
    }
}