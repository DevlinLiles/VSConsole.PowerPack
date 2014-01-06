namespace VSConsole.PowerPack.Core
{
    internal interface ITabExpansion
    {
        string[] GetExpansions(string line, string lastWord);
    }
}