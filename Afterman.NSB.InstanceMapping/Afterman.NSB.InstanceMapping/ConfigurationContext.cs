using System.Configuration;

namespace Afterman.NSB.InstanceMapping
{
    public static class ConfigurationContext
    {
        public static string AllowedServerPrefix => ConfigurationManager.AppSettings["AllowedServerPrefix"];
    }
}
