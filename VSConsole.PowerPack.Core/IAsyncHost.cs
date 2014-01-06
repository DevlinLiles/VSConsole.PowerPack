using System;

namespace VSConsole.PowerPack.Core
{
    public interface IAsyncHost : IHost
    {
        event EventHandler ExecuteEnd;
    }
}