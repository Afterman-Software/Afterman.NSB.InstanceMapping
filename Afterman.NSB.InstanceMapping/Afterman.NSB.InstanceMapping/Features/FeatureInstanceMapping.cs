using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Routing;

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

        private static readonly string AllowedServerPrefix = ConfigurationManager.AppSettings["AllowedServerPrefix"];

        protected override void Setup(FeatureConfigurationContext context)
        {
            var instancesToLoad = new List<EndpointInstance>();
            var nhibernateHelper = new NHibernateHelper();
            var instanceMappings = nhibernateHelper.GetAll<InstanceMapping>();

            RegisterCurrentEndpoint(context, instanceMappings, nhibernateHelper);

            foreach (var instanceMapping in instanceMappings.Where(m => m.IsEnabled))
            {
                if (Debugger.IsAttached)
                {
                    Console.WriteLine(
                        $"Registering mapping for '{instanceMapping.EndpointName}' at taget machine '{instanceMapping.TargetMachine}'");
                }
                instancesToLoad.Add(
                    new EndpointInstance(instanceMapping.EndpointName).AtMachine(instanceMapping.TargetMachine));
            }
            if (!instancesToLoad.Any()) return;

            var endpointInstances = context.Settings.Get<EndpointInstances>();
            endpointInstances.AddOrReplaceInstances("DefaultInstanceMappingKey", instancesToLoad);
        }

        private void RegisterCurrentEndpoint(FeatureConfigurationContext context,
            IEnumerable<InstanceMapping> instanceMappings, INHibernateHelper nHibernateHelper)
        {
            var endpointLogicalAddress = (LogicalAddress)context.Settings.Get("NServiceBus.LogicalAddress");
            var endpointName = endpointLogicalAddress.EndpointInstance.Endpoint;
            var machineName = endpointLogicalAddress.EndpointInstance.Properties["machine"];
            if (!string.IsNullOrEmpty(AllowedServerPrefix) && !machineName.ToLower().Contains(AllowedServerPrefix)) return;
            if (instanceMappings.Any(x => x.EndpointName == endpointName && x.TargetMachine == machineName)) return;

            nHibernateHelper.Add(new InstanceMapping
            {
                EndpointName = endpointName,
                TargetMachine = machineName,
                IsEnabled = true
            });
        }
    }
}
