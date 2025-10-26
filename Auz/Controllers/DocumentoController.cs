using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Documento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentoController : AuzControllerBase
    {
        private readonly ILogger _logger;
        private readonly IDocumentoService _documentoService;

        public DocumentoController(ILogger logger,
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
                _logger.LogError(ex.Message);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Ocorreu um erro na requisição." });
            }
        }
    }
}
