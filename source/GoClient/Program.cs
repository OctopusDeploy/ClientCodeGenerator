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
                    Console.Error.WriteLine("Usage: Octopus.Server.CodeGen.GoClient <swagger.json> <root_directory_of_the_source_repository>");
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

        private static void Run(string swaggerFile, string codeRoot)
        {
            ValidateParameters(swaggerFile, codeRoot);
            var schema = SwaggerParser.Parse(swaggerFile);

            var resourceGroups = schema.Apis.GroupBy(a => a.ResourceName)
                .Select(a => new {ResourceName = a.Key, Apis = a.ToArray()});

            using var template = CreateTemplate();
            if (template == null)
                return;

            foreach (var area in resourceGroups.Where(a => a.ResourceName == "ProjectGroup"))
            {
                var resources = schema.Resources.Where(r => r.Name == $"{area.ResourceName}Resource").ToArray();

                var code = Generate(template, SortApis(area.Apis), resources, new EnumDefinition[0]);
                if (code == null)
                    continue;

                var path = Path.Combine(codeRoot, "octopusdeploy", area.ResourceName.PascalToSnakeCase() + ".go");
                MergeInto(path, code);
            }
        }

     

        private static void MergeInto(string path, string code)
        {
            Console.WriteLine($"Merging into {path}");
            const string startCodeGen = "// Start of Generated Code";
            const string endCodeGen = "// End of Generated Code";

            var pre = new string[0];
            var post = new string[0];

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                pre = lines.TakeWhile(l => !l.Contains(startCodeGen)).ToArray();
                post = lines.SkipWhile(l => !l.Contains(endCodeGen)).Skip(1).ToArray();
            }

            var codeLines = code.Split('\n').Select(l => l.TrimEnd());

            var newContents = pre
                .Concat(new[] {startCodeGen})
                .Concat(codeLines)
                .Concat(new[] {endCodeGen})
                .Concat(post);
            File.WriteAllLines(path, newContents);
        }

        public static Template? CreateTemplate()
            => Template.Create(
                Extensions.GetEmbeddedResourceAsString<Program>("Resource.tt"),
                e =>
                {
                    e.Refs.Add(typeof(Program).Assembly.FullName);
                    e.Imports.Add(typeof(Program).Namespace);
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
            if (!File.Exists(readme) || !File.ReadAllText(readme).Contains("go-octopusdeploy"))
                throw new Exception($"Directory '{swaggerFile}' does not appear to be the root of the go-octopusdeploy repository. Looked for README.md that contains 'go-octopusdeploy'");
        }
        
        private static IReadOnlyList<ApiDefinition> SortApis(ApiDefinition[] apis)
        {
            return apis.Where(a => a.Method == ApiDefinition.GetMethod).OrderBy(a => a.)
                .Concat(apis.Where(a => a.Method == ApiDefinition.PostMethod))
                .Concat(apis.Where(a => a.Method == ApiDefinition.PutMethod))
                .Concat(apis.Where(a => a.Method == ApiDefinition.DeleteMethod))
                .ToArray();
        }
    }
}