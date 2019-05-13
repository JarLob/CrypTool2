using System;

namespace Cryptool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LocalizationAttribute : Attribute
    {
        public string ResourceClassPath { get; private set; }

        public LocalizationAttribute(string resourceClassPath)
        {
            ResourceClassPath = resourceClassPath;
        }
    }
}
