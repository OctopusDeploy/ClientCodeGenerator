namespace Octopus.Server.CodeGen
{
    public class PropertyDefinition
    {
        public const string IntType = "int";
        public const string LongType = "long";
        public const string DoubleType = "double";
        public const string BooleanType = "boolean";
        public const string StringType = "string";
        public const string LinkCollectionType = "LinkCollection";
        public const string DateType = "LocalDate";
        public const string DateTimeOffsetType = "DateTimeOffset";
        public const string ByteArrayType = "byte[]";
        public const string GuidType = "Guid";

        public PropertyDefinition(string name, string type, CollectionType collectionType)
        {
            Name = name;
            Type = type;
            CollectionType = collectionType;
        }

        public string Name { get; }
        public string Type { get; }
        public bool IsRequired => Name == "Name"; // TODO: Get this info the swagger doc and use that
        public CollectionType CollectionType { get; }
    }
}