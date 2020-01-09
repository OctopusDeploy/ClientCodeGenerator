using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Octopus.Server.CodeGen
{
    public class SwaggerParser
    {
        public static Schema Parse(string sourceFile)
        {
            var schema = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(sourceFile));

            var enums = GetEnums(schema).ToArray();
            var resources = GetResources(schema).ToArray();
            var apis = GetApis(schema).ToArray();

            return new Schema(enums, resources, apis);
        }

        static IEnumerable<EnumDefinition> GetEnums(JObject schema)
        {
            return from JProperty resource in schema.GetValueOrThrow("definitions").Children()
                from JProperty property in resource.Value.GetValueOrThrow("properties")
                let values = property.Value["enum"]?.Children().Select(c => (string?) c).ToArray()
                where values != null
                // TODO: Swagger does not define the name of the enum, so we use the property name
                select new EnumDefinition(property.Name, values)
                into ungrouped
                group ungrouped by ungrouped.Name
                into grp
                select grp.First();
        }

        static IEnumerable<ResourceDefinition> GetResources(JObject schema)
        {
            return from JProperty resource in schema.GetValueOrThrow("definitions").Children()
                let properties = resource.Value.GetValueOrThrow("properties")
                    .Children()
                    .Cast<JProperty>()
                    .Select(ParseProperty)
                    .ToArray()
                select new ResourceDefinition(resource.Name, properties);
        }

        static IEnumerable<ApiDefinition> GetApis(JObject schema)
        {
            return from JProperty path in schema.GetValueOrThrow("paths").Children()
                from JProperty method in path.Value.Children()
                select ParseApi(path.Name, method.Name, (JObject) method.Value);
        }

        static PropertyDefinition ParseProperty(JProperty property)
        {
            if (property.Name == "Links")
                return new PropertyDefinition("Links", PropertyDefinition.LinkCollectionType, CollectionType.None);

            var value = property.Value;
            if (value["enum"] != null)
                // TODO: Swagger does not define the name of the enum, so we use the property name
                return new PropertyDefinition(property.Name, property.Name, CollectionType.None);

            var (collectionType, type) = GetType(value);

            // TODO: Whether a property is writable is missing from the Swagger definition
            // TODO: Whether a property is nullable (e.g. Resource.LastModifiedOn) is missing from the Swagger definition

            return new PropertyDefinition(property.Name, type, CollectionType.None);
        }

        static Dictionary<string, string> formatMap = new Dictionary<string, string>
        {
            {"int32", PropertyDefinition.IntType},
            {"int64", PropertyDefinition.LongType},
            {"double", PropertyDefinition.DoubleType},
            {"byte", PropertyDefinition.ByteArrayType},
            {"uuid", PropertyDefinition.GuidType},
            {"date", PropertyDefinition.DateType}, // LocalDate
            {"date-time", PropertyDefinition.DateTimeOffsetType} // DateTimeOffset (maybe DateTime?)
        };

        static (CollectionType collectionType, string type) GetType(JToken token)
        {
            var format = (string?) token["format"];
            if (format != null)
                return formatMap.ContainsKey(format)
                    ? (CollectionType.None, formatMap[format])
                    : throw new Exception($"Format {format} not defined in the map");

            var type = (string?) token["type"];
            if (type == null)
            {
                var r = token.GetStringOrThrow("$ref");
                type = r.Substring(r.LastIndexOf("/") + 1);
            }

            if (type == "array")
                return (CollectionType.List, GetType(token.GetValueOrThrow("items")).type);

            if (type.StartsWith("ResourceCollection["))
                return (CollectionType.ResourceCollection, type.Substring(19, type.Length - 20));

            return (CollectionType.None, type);
        }

        static ApiDefinition ParseApi(string template, string method, JObject property)
        {
            var okResponse = property.GetValueOrThrow("responses")["200"] ?? property.GetValueOrThrow("responses")["201"];
            var responseType = okResponse?["schema"];

            if (responseType == null && method == ApiDefinition.GetMethod)
                Console.WriteLine($"Could not determine return type for {template} ({method.ToUpper()})");

            var returns = responseType == null
                ? (CollectionType.None, null)
                : GetType(responseType);

            return new ApiDefinition(
                property.GetStringOrThrow("operationId"),
                (string?) property.GetValueOrThrow("tags").First() ?? throw new Exception($"Api {template} does not have any tags"),
                template,
                method,
                (string?) property["summary"],
                (string?) property["description"],
                returns.collectionType,
                returns.type
            );
        }
    }

    public static class JsonExtensions
    {
        public static string GetStringOrThrow(this JToken obj, string name)
            => (string?) obj.GetValueOrThrow(name) ?? throw new Exception($"value of {name} property is null");

        public static JToken GetValueOrThrow(this JToken obj, string name)
            => obj[name] ?? throw new Exception($"{name} property not found");
    }
}