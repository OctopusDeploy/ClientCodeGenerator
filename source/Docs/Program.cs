using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Octopus.Server.CodeGen.Docs
{
    public class Program
    {
        
        public static int Main(string[] args)
        {
            try
            {
                if (args.Length != 2)
                {
                    Console.Error.WriteLine("Usage: Octopus.Server.CodeGen.Docs <swagger.json> <root_directory_of_the_docs_repository>");
                    return 3;
                }

                Run(args[0], args[1]);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 2;
            }
        }

        private static void Run(string swaggerFile, string docsRoot)
        {
            ValidateParameters(swaggerFile, docsRoot);
            var schema = SwaggerParser.Parse(swaggerFile);

            var areas = schema.Apis.GroupBy(a => a.ResourceName)
                .Select(a => new {Area = a.Key, Apis = a.ToArray()});
            
            using var template = CreateTemplate();
            if (template == null)
                return;

            foreach (var area in areas)
            {
                var resources = schema.Resources.Where(r => r.Name == $"{area.Area}Resource").ToArray();

                Generate(template, area.Apis, resources, new EnumDefinition[0]);
            }
        }

        public static Template? CreateTemplate()
            => Template.Create(
                Extensions.GetEmbeddedResourceAsString<Program>("Api.tt"),
                e =>
                {
                 //   e.Imports.Add(typeof(Helpers).Assembly.FullName);
               //     e.Refs.Add(typeof(Helpers).Namespace);
                }
                );

        public static string? Generate(Template template, IReadOnlyList<ApiDefinition> apis, IReadOnlyList<ResourceDefinition> resources, IReadOnlyList<EnumDefinition> enums)
        {
            return template.Run(new Dictionary<string, object>
            {
                {"apis", apis},
                {"resources", resources},
                {"enums", enums},
            });
        }

        private static void ValidateParameters(string swaggerFile, string docsRoot)
        {
            if (!File.Exists(swaggerFile))
                throw new Exception($"Swagger file '{swaggerFile}' does not exist");

            if (!Directory.Exists(docsRoot))
                throw new Exception($"Directory '{swaggerFile}' does not exist");

            var readme = Path.Combine(docsRoot, "README.md");
            if (!File.Exists(readme) || !File.ReadAllText(readme).Contains("https://octopus.com/docs"))
                throw new Exception($"Directory '{swaggerFile}' does not appear to be the root of the docs repository. Looked for README.md that contains 'https://octopus.com/docs'");
        }
    }
}