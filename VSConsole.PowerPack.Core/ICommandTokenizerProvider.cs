namespace Console.PowerPack.Core
{
    public interface ICommandTokenizerProvider
    {
        ICommandTokenizer Create(IHost host);
    }
}