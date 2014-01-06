using System;

namespace Console.PowerPack.Core
{
    public interface IAsyncHost : IHost
    {
        event EventHandler ExecuteEnd;
    }
}