using System.Linq;
using NServiceBus.Features;

namespace Afterman.NSB.InstanceMapping.Features
{
    using Repository;
    using Repository.Concepts;

    public class FeatureInstanceMapping
        : Feature
    {
        public FeatureInstanceMapping()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            if (!context.IsMsmqTransport() || context.IsSendOnlyEndpoint()) return;

            SqlHelper.CreateTableIfNotExists();
            
            var endpointInstances = context.GetEndpointInstances();
            var refresher = new AutoRefresher(endpointInstances);
            context.RegisterStartupTask(refresher);

            RegisterCurrentEndpoint(context);
        }

        private void RegisterCurrentEndpoint(FeatureConfigurationContext context)
        {
            var endpointName = context.EndpointName();
            var machineName = context.MachineName();
            
            var instanceMappings = SqlHelper.GetAll();
            if (instanceMappings.Any(x => x.EndpointName == endpointName && x.TargetMachine == machineName)) return;

            SqlHelper.Add(new InstanceMapping
            {
                EndpointName = endpointName,
                TargetMachine = machineName,
                IsEnabled = true
            });
        }
    }
}
