namespace Afterman.NSB.InstanceMapping.Repository.Concepts
{
    public class InstanceMapping
    {
        public int Id { get; set; }
        public string EndpointName { get; set; }
        public string TargetMachine { get; set; }
        public bool IsEnabled { get; set; }
    }
}
