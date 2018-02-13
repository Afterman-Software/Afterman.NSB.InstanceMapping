using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Routing;

namespace Afterman.NSB.InstanceMapping.Features
{
    using Repository;

    public class AutoRefresher : FeatureStartupTask
    {
        private readonly EndpointInstances _endpointInstances;
        private Timer _timer;

        public AutoRefresher(EndpointInstances endpointInstances)
        {
            _endpointInstances = endpointInstances;
        }

        protected override Task OnStart(IMessageSession session)
        {
            _timer = new Timer(
                callback: _ =>
                {
                    _endpointInstances.AddOrReplaceInstances("DefaultInstanceMappingKey", LoadInstances());
                },
                state: null,
                dueTime: TimeSpan.FromSeconds(30),
                period: TimeSpan.FromSeconds(30));
            return Task.CompletedTask;
        }

        private List<EndpointInstance> LoadInstances()
        {
            var instancesToLoad = new List<EndpointInstance>();
            var sqlHelper = new SqlHelper();
            var instanceMappings = sqlHelper.GetAll();

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

            return instancesToLoad;
        }

        protected override Task OnStop(IMessageSession session)
        {
            _timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
