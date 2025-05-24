using MemoryPack;
using MyGame.GameBackend.App.Core.Models;

namespace MyGame.GameBackend.App.Core.Messages
{
    public static class EnvelopeUtils
    {
        public static ProtocolEnvelope CreateErrorResponse(ProtocolEnvelope request, Exception ex)
        {
            return CreateResponse(request, new ErrorResponse
            {
                Code = "ERR_UNHANDLED",
                Message = ex.Message,
                Detail = ex.ToString()
            });
        }

        public static ProtocolEnvelope CreateErrorResponse(ProtocolEnvelope request, string message)
        {
            return CreateResponse(request, new ErrorResponse
            {
                Code = "ERR_GENERAL",
                Message = message
            });
        }

        public static ProtocolEnvelope CreateResponse<TResponse>(ProtocolEnvelope request, TResponse response) where TResponse : IMemoryPackable<TResponse>
        {
            return new ProtocolEnvelope
            {
                Kind = MessageKind.Response,
                MessageType = request.MessageType,
                RequestId = request.RequestId,
                Payload = MemoryPackSerializer.Serialize(response)
            };
        }
        public static ProtocolEnvelope CreateResponse(ProtocolEnvelope request, object response, Type responseType)
        {
            var method = typeof(MemoryPackSerializer)
                .GetMethods()
                .First(m => m.Name == "Serialize" && m.IsGenericMethod)
                .MakeGenericMethod(responseType);

            var payload = (byte[])method.Invoke(null, new[] { response })!;

            return new ProtocolEnvelope
            {
                Kind = MessageKind.Response,
                MessageType = request.MessageType,
                RequestId = request.RequestId,
                Payload = payload
            };
        }
    }

}
