﻿namespace VSConsole.PowerPack.Core
{
    public interface IWpfConsole : IConsole
    {
        object Content { get; }

        object VsTextView { get; }
    }
}