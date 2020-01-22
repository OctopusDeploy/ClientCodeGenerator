using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assent;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Server.CodeGen.Docs;

namespace Octopus.Server.CodeGen.Tests.Docs
{
    public class ApiTemplateFixture
    {
        [Test]
        public void Run()
        {
            using var template = Program.CreateTemplate() ?? throw new Exception("Could not create template");
            var result = RunTemplate(template);
            this.Assent(result);
        }

        [TestCase(1, 1_000)]
        [TestCase(1000, 2_000)]
        public void IsFast(int iterations, int maxMilliseconds)
        {
            var sw = Stopwatch.StartNew();
            using var template = Program.CreateTemplate() ?? throw new Exception("Could not create template");
            for (var x = 0; x < iterations; x++)
                RunTemplate(template)
                    .Should()
                    .NotBeNull();


            Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms");
            sw.ElapsedMilliseconds.Should().BeLessOrEqualTo(maxMilliseconds);
        }
        
        
        private string RunTemplate(Template? template)
        {
            IReadOnlyList<ApiDefinition> apis = new[]
            {
                new ApiDefinition(
                    "Example",
                    "Example",
                    "/api/example",
                    "get",
                    "The Summary",
                    "The Description",
                    CollectionType.ResourceCollection,
                    "ExampleResource"
                )
            };

            IReadOnlyList<ResourceDefinition> resources = new[]
            {
                new ResourceDefinition(
                    "ExampleResource",
                    new[]
                    {
                        new PropertyDefinition("Id", PropertyDefinition.StringType, CollectionType.None),
                        new PropertyDefinition("LinkedIds", PropertyDefinition.StringType, CollectionType.ResourceCollection),
                        new PropertyDefinition("Count", PropertyDefinition.IntType, CollectionType.None),
                        new PropertyDefinition("Flags", "Flags", CollectionType.List),
                        new PropertyDefinition("Child", "ExampleChildResource", CollectionType.None),
                    }
                ),
            };
            IReadOnlyList<EnumDefinition> enums = new[]
            {
                new EnumDefinition("Flags", new[] {"Green", "Orange"}),
            };
            
            return Program.Generate(template, apis, resources, enums);
        }


    }
}