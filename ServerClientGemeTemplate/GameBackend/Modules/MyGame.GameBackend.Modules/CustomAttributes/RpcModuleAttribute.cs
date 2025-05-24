namespace MyGame.GameBackend.App.Core.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RpcModuleAttribute : Attribute
    {
        public string Module { get; }
        public RpcModuleAttribute(string module) => Module = module;
    }

}
