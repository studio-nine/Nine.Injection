namespace Nine.Injection
{
    public class ContainerOptions
    {
        public bool ResolveFunc { get;set; }
        public bool ResolveLazy { get;set; } = true;
        
        internal static readonly ContainerOptions Default = new ContainerOptions();
    }
}