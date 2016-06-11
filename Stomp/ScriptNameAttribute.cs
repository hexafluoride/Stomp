using System;

namespace Stomp
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ScriptAliasAttribute : Attribute
    {
        public string Name { get; set; }

        public ScriptAliasAttribute(string name)
        {
            Name = name;
        }
    }
}

