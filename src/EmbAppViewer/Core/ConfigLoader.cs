using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EmbAppViewer.Core
{
    public static class ConfigLoader
    {
        private static readonly Deserializer Deserializer;

        static ConfigLoader()
        {
            Deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public static Config LoadConfig()
        {
            using (var reader = new StreamReader("config.yaml"))
            {
                var config = Deserializer.Deserialize<Config>(reader);
                return config;
            }
        }
    }
}
