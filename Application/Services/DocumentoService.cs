using Application.Interfaces;
using Application.Messaging.Request;
using Application.Messaging.Response;
using Application.Messaging.Response.Documento;
using Domain.Entidades;
using Domain.Enums;
using Infra.Repositories.Documentos;

namespace Application.Services
{
    public class DocumentoService : IDocumentoService
    {
        private readonly IDocumentoRepository _documentoRepository;
        private readonly IAwsService _awsService;

        public DocumentoService(IDocumentoRepository documentoRepository,
            IAwsService awsService)
        { 
            _awsService = awsService;   
            _documentoRepository = documentoRepository;
        }

        public async Task<ResponseBase> InserirDocumento(UploadDocumentoRequest uploadDocumentoRequest, Guid codigoUsuario, TipoEntidadeUpload tipoEntidadeUpload)
        {
            try
            {
                var fileBytes = Convert.FromBase64String(uploadDocumentoRequest.Base64Content);

                var contentType = uploadDocumentoRequest.ContentType ?? "application/octet-stream";

                var url = await _awsService.UploadFileAsync(fileBytes, uploadDocumentoRequest.FileName, contentType, ObterCaminhoFolder(tipoEntidadeUpload));

                var documento = new Documento
                {
                    TipoEntidade = ObterNomeEntidade(tipoEntidadeUpload),
                    CodigoEntidade = uploadDocumentoRequest.CodigoEntidade,
                    NomeArquivo = uploadDocumentoRequest.FileName,
                    CaminhoS3 = url,
                    Bucket = "auzys-documentos",
                    TipoConteudo = uploadDocumentoRequest.ContentType,
                    TamanhoBytes = fileBytes.Length,
                    UsuarioUpload = codigoUsuario,
                };

                _documentoRepository.Inserir(documento);

                return new ResponseBase
                {
                    Sucesso = true
                };
            }
            catch (Exception ex)
            {
                throw; // Por algum motivo esse maravilha não ta estourando exception sem isso 
            }
        }

        public async Task<DadosDocumentoResponse> ObterDocumento(Guid codigoDocumento)
        {
            var dadosDocumento = _documentoRepository.ObterCaminhoPorCodigo(codigoDocumento);

            var documento = await _awsService.GetFileAsync(dadosDocumento.CaminhoS3);

            return new DadosDocumentoResponse 
            { 
                DadosDocumento = dadosDocumento,
                Documento = documento
            };
        } 

        private string ObterNomeEntidade(TipoEntidadeUpload tipoEntidadeUpload)
        {
            return tipoEntidadeUpload switch
            {
                TipoEntidadeUpload.Usuario => "Usuario",
                TipoEntidadeUpload.Atendimento => "Atendimento",
                _ => String.Empty,
            };
        }

        private string ObterCaminhoFolder(TipoEntidadeUpload tipoEntidadeUpload)
        {
            return tipoEntidadeUpload switch
            {
                TipoEntidadeUpload.Usuario => "Documentos",
                TipoEntidadeUpload.Atendimento => "Atendimentos",
                _ => String.Empty,
            };
        }
    }
}
