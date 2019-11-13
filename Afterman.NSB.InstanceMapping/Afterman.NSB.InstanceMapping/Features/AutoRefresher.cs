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
    using NServiceBus.Logging;
    using Repository;

    public class AutoRefresher : FeatureStartupTask
    {
        private readonly EndpointInstances _endpointInstances;
        private Timer _timer;
        private ILog _log = LogManager.GetLogger<AutoRefresher>();

        public AutoRefresher(EndpointInstances endpointInstances)
        {
            _endpointInstances = endpointInstances;
        }

        protected override Task OnStart(IMessageSession session)
        {
            _log.Info("Initializing DatabaseInstanceMapping AutoRefresher");
            // load here without any error handling because we will fail later in initialization if InstanceMappings aren't present
            _endpointInstances.AddOrReplaceInstances("InstanceMappings", LoadInstances());

            _timer = new Timer(
                callback: _ =>
                {
                    try
                    {
                        _log.Info("Refreshing endpoint instances from the database");
                        _endpointInstances.AddOrReplaceInstances("InstanceMappings", LoadInstances());
                    }
                    catch (Exception e)
                    {
                        _log.Error("", e);
                    }
                },
                state: null,
                dueTime: TimeSpan.FromSeconds(30), // load after 30 seconds
                period: TimeSpan.FromSeconds(30)); // repeat every 30 seconds
            return Task.CompletedTask;
        }


        private List<EndpointInstance> LoadInstances()
        {
            var instancesToLoad = new List<EndpointInstance>();
            var instanceMappings = SqlHelper.GetAll();

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
