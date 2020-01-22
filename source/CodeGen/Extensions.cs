using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Octopus.Server.CodeGen
{
    public static class Extensions
    {
        public static string GetStringOrThrow(this JToken obj, string name)
            => (string?) obj.GetValueOrThrow(name) ?? throw new Exception($"value of {name} property is null");

        public static JToken GetValueOrThrow(this JToken obj, string name)
            => obj[name] ?? throw new Exception($"{name} property not found");

        public static string GetEmbeddedResourceAsString(this object parent, string subName)
            => GetEmbeddedResourceAsString(parent.GetType(), subName);
        
        public static string GetEmbeddedResourceAsString<T>(string subName)
            => GetEmbeddedResourceAsString(typeof(T), subName);

        public static string GetEmbeddedResourceAsString(this Type type, string subName)
        {
            var name = type.Namespace + "." + subName;

            using var stream = type.Assembly.GetManifestResourceStream(name);

            if (stream == null)
                throw new Exception($"Could not find embedded resource {name}");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static string GetSingular(this string value)
        {
            if (value.EndsWith("s"))
                return value.Substring(0, value.Length - 1);
            return value;
        }

        public static string PascalToSnakeCase(this string str)
            => Regex.Replace(str, "(?<=[a-z])(?=[A-Z])", "_").ToLower();

        public static string PascalToCamelCase(this string str)
            => char.ToLower(str[0]) + str.Substring(1);
    }
}