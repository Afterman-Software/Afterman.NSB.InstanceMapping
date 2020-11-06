using Afterman.NSB.InstanceMapping.Constants;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Routing;

namespace Afterman.NSB.InstanceMapping.Features
{
    public static class FeatureConfigurationContextExtensions
    {
        public static bool IsMsmqTransport(this FeatureConfigurationContext context)
        {
            return context.Settings.Get(NServiceBusSettings.TransportDefinition) is MsmqTransport;
        }

        public static bool IsSendOnlyEndpoint(this FeatureConfigurationContext context)
        {
            return context.Settings.Get<bool>(NServiceBusSettings.SendOnly);
        }

        public static LogicalAddress LogicalAddress(this FeatureConfigurationContext context)
        {
            return context.Settings.Get<LogicalAddress>(NServiceBusSettings.LogicalAddress);
        }

        public static string EndpointName(this FeatureConfigurationContext context)
        {
            return context.LogicalAddress().EndpointInstance.Endpoint;
        }

        public static string MachineName(this FeatureConfigurationContext context)
        {
            return context.LogicalAddress().EndpointInstance.Properties[NServiceBusSettings.Machine];
        }

        public static EndpointInstances GetEndpointInstances(this FeatureConfigurationContext context)
        {
            return context.Settings.Get<EndpointInstances>();
        }
    }
}
