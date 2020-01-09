using System.Collections.Generic;

namespace Octopus.Server.CodeGen
{
    public class ResourceDefinition
    {
        public ResourceDefinition(string name, IReadOnlyList<PropertyDefinition> properties)
        {
            Name = name;
            Properties = properties;
        }

        public string Name { get; }
        public IReadOnlyList<PropertyDefinition> Properties { get; }
    }
}