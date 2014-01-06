namespace VSConsole.PowerPack.Core
{
    public interface ICommandExpansionProvider
    {
        ICommandExpansion Create(IHost host);
    }
}