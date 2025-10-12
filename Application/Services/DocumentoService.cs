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

        public async Task<ResponseBase> InserirDocumento(UploadDocumentoRequest uploadDocumentoRequest, Guid codigoUsuario, TipoEntidadeUpload tipoEntidadeUpload, TipoDocumento tipoDocumento)
        {
            try
            {
                using var ms = new MemoryStream();

                await uploadDocumentoRequest.File.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                var contentType = uploadDocumentoRequest.File.ContentType ?? "application/octet-stream";

                var url = await _awsService.UploadFileAsync(fileBytes, uploadDocumentoRequest.File.FileName, contentType, ObterCaminhoFolder(tipoEntidadeUpload));

                var documento = new Documento
                {
                    TipoEntidade = ObterNomeEntidade(tipoEntidadeUpload),
                    CodigoEntidade = uploadDocumentoRequest.CodigoEntidade,
                    NomeArquivo = uploadDocumentoRequest.File.FileName,
                    CaminhoS3 = url,
                    Bucket = "auzys-documentos",
                    TipoConteudo = contentType,
                    TamanhoBytes = fileBytes.Length,
                    UsuarioUpload = codigoUsuario,
                    TipoDocumento = tipoDocumento
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

        public async Task<DadosDocumentoResponse> ObterFotoPerfil(Guid codigoEntidade)
        {
            var dadosDocumento = _documentoRepository.ObterCaminhoPorEntidade(codigoEntidade, TipoDocumento.FotoPerfil);

            var fotoPerfil = dadosDocumento is not null ? await _awsService.GetFileAsync(dadosDocumento.CaminhoS3) : null;

            return new DadosDocumentoResponse
            {
                DadosDocumento = dadosDocumento,
                Documento = fotoPerfil
            };
        }

        private string ObterNomeEntidade(TipoEntidadeUpload tipoEntidadeUpload)
        {
            return tipoEntidadeUpload switch
            {
                TipoEntidadeUpload.Usuario => "Usuario",
                TipoEntidadeUpload.Atendimento => "Atendimento",
                TipoEntidadeUpload.Perfil => "Perfil",
                _ => String.Empty,
            };
        }

        private string ObterCaminhoFolder(TipoEntidadeUpload tipoEntidadeUpload)
        {
            return tipoEntidadeUpload switch
            {
                TipoEntidadeUpload.Usuario => "Documentos",
                TipoEntidadeUpload.Atendimento => "Atendimentos",
                TipoEntidadeUpload.Perfil => "Perfil",
                _ => String.Empty,
            };
        }
    }
}
