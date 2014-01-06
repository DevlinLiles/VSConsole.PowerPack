using System;

namespace Console.PowerPack.Core
{
    public interface IWpfConsoleService
    {
        IWpfConsole CreateConsole(IServiceProvider sp, string contentTypeName, string hostName);

        object TryCreateCompletionSource(object textBuffer);

        object GetClassifier(object textBuffer);
    }
}