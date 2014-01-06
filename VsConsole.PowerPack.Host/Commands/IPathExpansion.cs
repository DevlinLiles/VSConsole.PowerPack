namespace VSConsole.PowerPack.Core
{
    internal interface IPathExpansion : ITabExpansion
    {
        SimpleExpansion GetPathExpansions(string line);
    }
}