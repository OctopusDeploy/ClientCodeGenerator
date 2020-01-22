using System;

namespace Octopus.Server.CodeGen.Docs
{
    public static class Helpers
    {
        public static string FormatType(string type, CollectionType collectionType)
        {
            switch (collectionType)
            {
                case CollectionType.None:
                    return type;
                case CollectionType.List:
                    return type + "[]";
                case CollectionType.ResourceCollection:
                    return $"[ResourceCollection](ResourceCollection)<{type}>";
                default:
                    throw new ArgumentOutOfRangeException(nameof(collectionType), collectionType, null);
            }
            
        }
    }
}