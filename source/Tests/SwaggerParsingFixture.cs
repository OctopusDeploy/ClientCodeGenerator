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
                using var stream = typeof(SwaggerParsingFixture).Assembly.GetManifestResourceStream(typeof(SwaggerParsingFixture).Namespace + ".FullApiSwagger.json")
                             ?? throw new Exception("Test swagger file not found as an embedded resource");
                using(var temp = File.OpenWrite(filename))
                    stream.CopyTo(temp);

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