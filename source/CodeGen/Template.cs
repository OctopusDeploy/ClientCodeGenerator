using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Mono.TextTemplating;

namespace Octopus.Server.CodeGen
{
    public class Template : IDisposable
    {
        private readonly TemplateGenerator generator;
        private readonly CompiledTemplate compiledTemplate;

        private Template(TemplateGenerator generator, CompiledTemplate compiledTemplate)
        {
            this.generator = generator;
            this.compiledTemplate = compiledTemplate;
        }

        public static Template? Create(string template, Action<TemplateGenerator> configure = null)
        {
            var generator = new TemplateGenerator();
            generator.Refs.Add(typeof(ApiDefinition).Assembly.FullName);
            generator.Refs.Add(typeof(Console).Assembly.FullName);
            generator.Refs.Add(typeof(CodeDomProvider).Assembly.FullName);
            generator.Refs.Add(typeof(System.Linq.Enumerable).Assembly.FullName);
            generator.Imports.Add(typeof(Console).Namespace);
            generator.Imports.Add(typeof(List<>).Namespace);
            generator.Imports.Add(typeof(System.Linq.Enumerable).Namespace);
            generator.Imports.Add(typeof(SwaggerParser).Namespace);
            configure?.Invoke(generator);
            generator.GetOrCreateSession();
            
            var compiledTemplate = generator.CompileTemplate(template);

            if (PrintErrors(generator))
            {
                compiledTemplate?.Dispose();
                return null;
            }

            return new Template(generator, compiledTemplate);
        }

        private static bool PrintErrors(TemplateGenerator generator)
        {
            if (!generator.Errors.HasErrors)
                return false;

            foreach (var error in generator.Errors)
                Console.Error.WriteLine(error);

            return true;
        }

        public string? Run(Dictionary<string, object> parameters)
        {
            generator.Errors.Clear();
            var session = generator.GetOrCreateSession();
            session.Clear();
            foreach (var parameter in parameters)
                session[parameter.Key] = parameter.Value;

            var result = compiledTemplate.Process();
            PrintErrors(generator);
            return result;
        }

        public void Dispose()
        {
            compiledTemplate.Dispose();
        }
    }
}