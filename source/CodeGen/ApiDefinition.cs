namespace Octopus.Server.CodeGen
{
    public class ApiDefinition 
    {
        public const string GetMethod = "get";
        public const string PostMethod = "post";
        public const string PutMethod = "put";
        public const string DeleteMethod = "delete";
	
        public ApiDefinition(
            string id, 
            string resourceNamePlural, 
            string template, 
            string method, 
            string? summary, 
            string? description, 
            CollectionType returnsCollectionType, 
            string? returns)
        {
            Id = id;
            ResourceNamePlural = resourceNamePlural;
            Template = template;
            Method = method;
            Summary = summary;
            Description = description;
            ReturnCollectionType = returnsCollectionType;
            Returns = returns;
        }

        public string Id { get; }
        public string ResourceName => ResourceNamePlural.GetSingular();
        public string ResourceNamePlural { get; }
        public string Template { get; }
        public string Method { get; }
        public string? Summary { get; }
        public string? Description { get; }
        public CollectionType ReturnCollectionType { get; }
        public string? Returns { get; }
    }
}