using Application.Interfaces;
using Application.Messaging.Request;
using Application.Messaging.Request.Documento;
using Application.Messaging.Response;
using Application.Messaging.Response.Documento;
using Domain.Entidades;
using Domain.Enums;
using Infra.Repositories.Agendamentos;
using Infra.Repositories.Documentos;

namespace Application.Services
{
    public class DocumentoService : IDocumentoService
    {
        private readonly IDocumentoRepository _documentoRepository;
        private readonly IAwsService _awsService;
        private readonly IAgendamentoRepository _agendamentoRepository;

        public DocumentoService(IDocumentoRepository documentoRepository,
            IAwsService awsService,
            IAgendamentoRepository agendamentoRepository)
        { 
            _awsService = awsService;   
            _documentoRepository = documentoRepository;
            _agendamentoRepository = agendamentoRepository;
        }

        public async Task<ResponseBase> InserirDocumento(UploadDocumentoRequest uploadDocumentoRequest, Guid codigoUsuario, TipoEntidadeUpload tipoEntidadeUpload, TipoDocumento tipoDocumento, bool ehAgendamento = false)
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

                if (ehAgendamento)
                {
                    var listaDocumentos = new List<Documento>();
                    listaDocumentos.Add(documento);

                    var agendamento = _agendamentoRepository.Obter(uploadDocumentoRequest.CodigoEntidade);

                    documento.TipoEntidade = "Atendimento";
                    documento.CodigoEntidade = agendamento.CodigoAtendimento;

                    listaDocumentos.Add(documento);

                    _documentoRepository.InserirListagem(listaDocumentos);

                    return new ResponseBase
                    {
                        Sucesso = true
                    };
                }

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

        public ListarDocumentosResponse Listar(ListarDocumentosRequest request)
        {
            var pagina = Math.Max(1, request.Pagina.GetValueOrDefault(1));

            var itensPorPagina = request.ItensPorPagina.GetValueOrDefault(10);
            if (itensPorPagina <= 0) itensPorPagina = 10;

            var documentos = _documentoRepository.ObterDocumentosPorCodigoEntidade(request.CodigoEntidade, pagina, itensPorPagina);

            var totalizador = _documentoRepository.ObterTotalizadorDocumentosPorCodigoEntidade(request.CodigoEntidade);
            var totalItens = totalizador?.Count ?? 0;

            var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
            totalPaginas = Math.Max(1, totalPaginas);

            return new ListarDocumentosResponse
            {
                Documentos =documentos,
                TotalPaginas = totalPaginas,
                Itens = totalItens
            };
        }

        private string ObterNomeEntidade(TipoEntidadeUpload tipoEntidadeUpload)
        {
            return tipoEntidadeUpload switch
            {
                TipoEntidadeUpload.Usuario => "Usuario",
                TipoEntidadeUpload.Atendimento => "Atendimento",
                TipoEntidadeUpload.Perfil => "Perfil",
                TipoEntidadeUpload.Agendamento => "Agendamento",
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
                TipoEntidadeUpload.Agendamento => "Agendamento",
                _ => String.Empty,
            };
        }
    }
}
