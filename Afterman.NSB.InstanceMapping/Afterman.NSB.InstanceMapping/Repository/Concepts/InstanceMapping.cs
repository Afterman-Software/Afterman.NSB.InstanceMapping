namespace Afterman.NSB.InstanceMapping.Repository.Concepts
{
    public class InstanceMapping
    {
        public virtual int Id { get; set; }
        public virtual string EndpointName { get; set; }
        public virtual string TargetMachine { get; set; }
        public virtual bool IsEnabled { get; set; }
    }
}
