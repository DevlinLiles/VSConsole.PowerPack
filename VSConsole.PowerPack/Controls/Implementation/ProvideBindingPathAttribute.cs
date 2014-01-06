using System;
using System.Text;
using Microsoft.VisualStudio.Shell;

namespace VSConsole.PowerPack.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideBindingPathAttribute : RegistrationAttribute
    {
        public string SubPath { get; set; }

        private static string GetPathToKey(RegistrationContext context)
        {
            return "BindingPaths\\" + context.ComponentType.GUID.ToString("B").ToUpperInvariant();
        }

        public override void Register(RegistrationContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            using (Key key = context.CreateKey(GetPathToKey(context)))
            {
                var stringBuilder = new StringBuilder(context.ComponentPath);
                if (!string.IsNullOrEmpty(SubPath))
                {
                    stringBuilder.Append("\\");
                    stringBuilder.Append(SubPath);
                }
                key.SetValue(stringBuilder.ToString(), string.Empty);
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            context.RemoveKey(GetPathToKey(context));
        }
    }
}