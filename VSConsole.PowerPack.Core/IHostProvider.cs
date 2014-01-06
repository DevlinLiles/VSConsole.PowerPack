namespace VSConsole.PowerPack.Core
{
    public interface IHostProvider
    {
        IHost CreateHost(IConsole console);
    }
}