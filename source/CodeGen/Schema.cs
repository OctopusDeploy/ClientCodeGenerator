using System.Collections.Generic;

namespace Octopus.Server.CodeGen
{
    public class Schema {

        public Schema(
            IReadOnlyList<EnumDefinition> enums,
            IReadOnlyList<ResourceDefinition> resources,
            IReadOnlyList<ApiDefinition> apis
        )
        {
            Enums = enums;
            Resources = resources;
            Apis = apis;
        }
        public IReadOnlyList<EnumDefinition> Enums { get; }
        public IReadOnlyList<ResourceDefinition> Resources { get; }
        public IReadOnlyList<ApiDefinition> Apis {get; }
	
    }
}