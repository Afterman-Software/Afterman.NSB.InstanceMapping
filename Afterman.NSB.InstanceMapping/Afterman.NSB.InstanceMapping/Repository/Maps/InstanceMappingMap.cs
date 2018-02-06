using FluentNHibernate.Mapping;

namespace Afterman.NSB.InstanceMapping.Repository.Maps
{
    using Concepts;

    public class InstanceMappingMap :
        ClassMap<InstanceMapping>
    {
        public InstanceMappingMap()
        {
            Id(x => x.Id)
                .GeneratedBy
                .Identity();

            Map(x => x.EndpointName);
            Map(x => x.TargetMachine);
            Map(x => x.IsEnabled)
                .Not
                .Nullable();
        }
    }
}
