using System.Collections.Generic;

namespace Octopus.Server.CodeGen
{
    public class EnumDefinition {
        public EnumDefinition(string name, IReadOnlyList<string> values)
        {
            Name = name;
            Values = values;
        }
	
        public string Name {get; }
        public IReadOnlyList<string> Values { get;  }
    }
}