using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EmbAppViewer.Core
{
    public static class ConfigLoader
    {
        private static readonly IDeserializer Deserializer;

        static ConfigLoader()
        {
            Deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public static Config LoadConfig()
        {
            var configFile = "config.yaml";
            if (File.Exists(configFile))
            {
                using (var reader = new StreamReader(configFile))
                {
                    var config = Deserializer.Deserialize<Config>(reader);
                    return config;
                }
            }
            return new Config();
        }
    }
}
