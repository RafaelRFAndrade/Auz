using Amazon.S3;
using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Documento;
using Auz.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentoController : AuzControllerBase
    {
        private readonly ILokiLogger _logger;
        private readonly IDocumentoService _documentoService;

        public DocumentoController(ILokiLogger logger,
            IDocumentoService documentoService)
        {
            _logger = logger;
            _documentoService = documentoService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Listar([FromQuery] ListarDocumentosRequest request)
        {
            try
            {
                var atendimentos = _documentoService.Listar(request);

                return Ok(atendimentos);
            }
            catch (AuzException ex)
            {
                return BadRequest(new { Sucesso = false, Mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar requisição: {ex.Message}", null, ex);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Ocorreu um erro na requisição." });
            }
        }

        [HttpGet("{codigoDocumento}/download")]
        [Authorize]
        public async Task<IActionResult> GetDocumento(Guid codigoDocumento)
        {
            try
            {
                var dadosDocumento = await _documentoService.ObterDocumento(codigoDocumento);
                var contentType = dadosDocumento.DadosDocumento.TipoConteudo;
                return File(dadosDocumento.Documento, contentType, dadosDocumento.DadosDocumento.NomeArquivo);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(new { Sucesso = false, Mensagem = "Arquivo não encontrado." });
            }
            catch (AuzException ex)
            {
                return BadRequest(new { Sucesso = false, Mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar requisição: {ex.Message}", null, ex);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Ocorreu um erro na requisição." });
            }
        }
    }
}
