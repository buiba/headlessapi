using System;

namespace EPiServer.DefinitionsApi.Internal
{
    /// <summary>
    /// This is an internal attribute used for generating API documentation
    /// </summary>
    /// <exclude />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class ApiDefinitionAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
