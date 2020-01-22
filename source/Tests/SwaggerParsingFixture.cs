using System;
using System.IO;
using Assent;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Octopus.Server.CodeGen.Tests
{
    public class SwaggerParsingFixture
    {
        [Test]
        public void FullApi()
        {
            var filename = Path.GetTempFileName();
            try
            {
                File.WriteAllText(filename, this.GetEmbeddedResourceAsString("FullApiSwagger.json"));

                var result = SwaggerParser.Parse(filename);
                
                this.Assent(JsonConvert.SerializeObject(result, Formatting.Indented));
            }
            finally
            {
                File.Delete(filename);
            }
        }
    }
}