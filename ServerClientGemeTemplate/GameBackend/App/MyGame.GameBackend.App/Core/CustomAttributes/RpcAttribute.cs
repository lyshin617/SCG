using MyGame.GameBackend.App.Core.Models;
using System;
namespace MyGame.GameBackend.App.Core.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RpcAttribute : Attribute
    {
        public string Action { get; }
        public RpcKind Kind { get; }
        public RpcAttribute(string action) => Action = action;
    }
    public enum RpcKind
    {
        None,
        Request,
        Response,
        Event,
    }
}
