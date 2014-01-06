using System;
using System.ComponentModel.Composition;

namespace Console.PowerPack.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [MetadataAttribute]
    public class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute(string displayName)
        {
            UtilityMethods.ThrowIfArgumentNullOrEmpty(displayName);
            DisplayName = displayName;
        }

        public string DisplayName { get; private set; }
    }
}