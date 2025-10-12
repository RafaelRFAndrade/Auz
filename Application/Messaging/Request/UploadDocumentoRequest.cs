using Microsoft.AspNetCore.Http;

namespace Application.Messaging.Request
{
    public class UploadDocumentoRequest
    {
        public IFormFile File { get; set; } = null!;
        public Guid CodigoEntidade { get; set; }
    }
}
