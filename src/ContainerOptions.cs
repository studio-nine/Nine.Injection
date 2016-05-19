namespace Nine.Injection
{
    using System.Collections.Generic;

    public class ContainerOptions
    {
        public bool ResolveFunc { get;set; }
        public bool ResolveLazy { get;set; } = true;

        /// <summary>
        /// Gets or sets the equality comparer to compare the equality of parameter objects.
        /// </summary>
        public IEqualityComparer<object> EqualityComparer { get; set; }

        internal static readonly ContainerOptions Default = new ContainerOptions();
    }
}