using System;
namespace MyGame.GameBackend.App.Core.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RpcAttribute : Attribute
    {
        public string Action { get; }
        public RpcAttribute(string action) => Action = action;
    }

}
