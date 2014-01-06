using System.Windows.Media;

namespace Console.PowerPack.Core
{
    public interface IConsole
    {
        IHost Host { get; set; }

        IConsoleDispatcher Dispatcher { get; }

        int ConsoleWidth { get; }

        void Write(string text);

        void WriteLine(string text);

        void Write(string text, Color? foreground, Color? background);

        void Clear();
    }
}