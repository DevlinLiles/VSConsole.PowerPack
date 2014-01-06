namespace VSConsole.PowerPack.Core
{
    public interface ICommandTokenizerProvider
    {
        ICommandTokenizer Create(IHost host);
    }
}