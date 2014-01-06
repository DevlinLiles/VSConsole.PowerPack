namespace VSConsole.PowerPack.Core
{
    internal class CommandExpansionProvider : ICommandExpansionProvider
    {
        public ICommandExpansion Create(IHost host)
        {
            ITabExpansion tabExpansion = host as ITabExpansion;
            if (tabExpansion == null)
                return (ICommandExpansion)null;
            else
                return this.CreateTabExpansion(tabExpansion);
        }

        protected virtual ICommandExpansion CreateTabExpansion(ITabExpansion tabExpansion)
        {
            return (ICommandExpansion)new CommandExpansion(tabExpansion);
        }
    }
}