namespace VSConsole.PowerPack.Core
{
    using System;

    internal static class CommonExtensionMethods
    {
        public static void Raise(this EventHandler ev, object sender = null, EventArgs e = null)
        {
            if (ev == null)
                return;
            ev(sender, e);
        }

        public static void Raise<Args>(this EventHandler<Args> ev, object sender = null, Args e = null) where Args : EventArgs
        {
            if (ev == null)
                return;
            ev(sender, e);
        }

        public static T GetService<T>(this IServiceProvider sp, Type serviceType) where T : class
        {
            return (T)sp.GetService(serviceType);
        }
    }

}