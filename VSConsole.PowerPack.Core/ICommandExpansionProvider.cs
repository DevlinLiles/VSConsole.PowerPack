namespace Console.PowerPack.Core
{
    public interface ICommandExpansionProvider
    {
        ICommandExpansion Create(IHost host);
    }
}