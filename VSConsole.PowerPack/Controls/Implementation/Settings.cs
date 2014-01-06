using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSConsole.PowerPack.Core
{
    public class Settings
    {
        private const string CollectionPath = "Console.PowerPack";
        private const string ActiveHostPropertyName = "ActiveHost";
        private const string DefActiveHost = "Microsoft.VisualStudio.Console.PowerPack.Host.PowerShell";

        public static void GetDefaultHost(IServiceProvider sp, out string defHost)
        {
            defHost = null;
            var service = sp.GetService<IVsSettingsManager>(typeof (SVsSettingsManager));
            if (service != null)
            {
                IVsSettingsStore store;
                ErrorHandler.ThrowOnFailure(service.GetReadOnlySettingsStore(2U, out store));
                int pfExists;
                ErrorHandler.ThrowOnFailure(store.CollectionExists("Console.PowerPack", out pfExists));
                if (pfExists != 0)
                    ErrorHandler.ThrowOnFailure(store.GetStringOrDefault("Console.PowerPack", "ActiveHost",
                        "Microsoft.VisualStudio.Console.PowerPack.Host.PowerShell", out defHost));
            }
            if (defHost != null)
                return;
            defHost = "Microsoft.VisualStudio.Console.PowerPack.Host.PowerShell";
        }

        public static void SetDefaultHost(IServiceProvider sp, string defHost)
        {
            if (string.IsNullOrEmpty(defHost))
                return;
            var service = sp.GetService<IVsSettingsManager>(typeof (SVsSettingsManager));
            if (service == null)
                return;
            IVsWritableSettingsStore writableStore;
            ErrorHandler.ThrowOnFailure(service.GetWritableSettingsStore(2U, out writableStore));
            int pfExists;
            ErrorHandler.ThrowOnFailure(writableStore.CollectionExists("Console.PowerPack", out pfExists));
            if (pfExists == 0)
                ErrorHandler.ThrowOnFailure(writableStore.CreateCollection("Console.PowerPack"));
            ErrorHandler.ThrowOnFailure(writableStore.SetString("Console.PowerPack", "ActiveHost", defHost));
        }
    }
}