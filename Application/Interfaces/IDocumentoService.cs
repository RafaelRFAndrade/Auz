using Application.Messaging.Request;
using Application.Messaging.Response;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IDocumentoService
    {
        Task<ResponseBase> InserirDocumento(UploadDocumentoRequest uploadDocumentoRequest, Guid codigoUsuario, TipoEntidadeUpload tipoEntidadeUpload);
    }
}
