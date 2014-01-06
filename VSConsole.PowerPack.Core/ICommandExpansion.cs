namespace VSConsole.PowerPack.Core
{
    public interface ICommandExpansion
    {
        SimpleExpansion GetExpansions(string line, int caretIndex);
    }
}