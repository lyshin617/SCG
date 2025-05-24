using MemoryPack;
using MyGame.GameBackend.App.Core.Models;

namespace MyGame.GameBackend.App.Core.Messages
{
    public static class EnvelopeUtils
    {
        public static ProtocolEnvelope CreateErrorResponse(ProtocolEnvelope request, Exception ex)
        {
            return CreateErrorResponse(request, new ErrorResponse
            {
                Code = "ERR_UNHANDLED",
                Message = ex.Message,
                Detail = ex.ToString()
            });
        }

        public static ProtocolEnvelope CreateErrorResponse(ProtocolEnvelope request, string message)
        {
            return CreateErrorResponse(request, new ErrorResponse
            {
                Code = "ERR_GENERAL",
                Message = message
            });
        }

        private static ProtocolEnvelope CreateErrorResponse(ProtocolEnvelope request, ErrorResponse error)
        {
            return new ProtocolEnvelope
            {
                Kind = MessageKind.Response,
                MessageType = new MessageType
                {
                    Action = request.MessageType.Action
                },
                RequestId = request.RequestId,
                Payload = MemoryPackSerializer.Serialize(error)
            };
        }
    }

}
