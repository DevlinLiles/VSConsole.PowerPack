using System;
using System.ComponentModel.Composition;

namespace Console.PowerPack.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [MetadataAttribute]
    public class HostNameAttribute : Attribute
    {
        public HostNameAttribute(string hostName)
        {
            UtilityMethods.ThrowIfArgumentNull(hostName);
            HostName = hostName;
        }

        public string HostName { get; private set; }
    }
}