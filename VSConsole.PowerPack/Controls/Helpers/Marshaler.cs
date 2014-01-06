using System;
using Microsoft.VisualStudio.Shell;

namespace VSConsole.PowerPack.Core
{
    public class Marshaler<T>
    {
        protected readonly T _impl;

        protected Marshaler(T impl)
        {
            _impl = impl;
        }

        private static ThreadHelper ThreadHelper
        {
            get { return ThreadHelper.Generic; }
        }

        protected void Invoke(Action action)
        {
            ThreadHelper.Invoke(action);
        }

        protected TResult Invoke<TResult>(Func<TResult> func)
        {
            return ThreadHelper.Invoke(func);
        }
    }
}