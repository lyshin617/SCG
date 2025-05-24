
namespace MyGame.GameBackend.App.Core.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RpcAttribute : Attribute
    {
        public string Action { get; }
        public RpcKind Kind { get; }
        public RpcAttribute(string action, RpcKind Kind) => Action = action;
    }
    public enum RpcKind
    {
        Response,
        Event,
    }
}
