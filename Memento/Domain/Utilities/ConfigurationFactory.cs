using System.IO;
using Microsoft.Extensions.Configuration;

namespace Domain.Utilities
{
    public class ConfigurationFactory
    {
        private static IConfigurationRoot configuration;


        public static string GetConfigurationValue(string key)
        {
            return GetConfigurationValue("Generic", key);
        }

        public static string GetConfigurationValue(string section, string key) {
            var config = GetConfiguration();
            config.GetSection(section);
            return config.GetConnectionString("DatabaseConnection");
        }

        private static IConfigurationRoot GetConfiguration() {
            if (configuration == null) {
                configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(@Directory.GetCurrentDirectory() + "/../Memento/appsettings.json").Build();
            }
            return configuration;
        }
    }
}
