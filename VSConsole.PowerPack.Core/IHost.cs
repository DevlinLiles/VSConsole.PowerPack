namespace VSConsole.PowerPack.Core
{
    public interface IHost
    {
        string Prompt { get; }

        bool Execute(string command);

        void Abort();
    }
}