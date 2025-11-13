using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Agendamento;
using Application.Services;
using Auz.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AgendamentoController : AuzControllerBase
    {
        private readonly ILokiLogger _logger;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IDocumentoService _documentoService;

        public AgendamentoController(ILokiLogger logger,
            IAgendamentoService agendamentoService,
            IDocumentoService documentoService)
        {
            _logger = logger;
            _agendamentoService = agendamentoService;
            _documentoService = documentoService;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Cadastrar(CadastroAgendamentoRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _agendamentoService.Cadastrar(request, codigoUsuario);

                return Created();
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

        [HttpGet]
        [Authorize]
        public IActionResult Listar([FromQuery]AgendamentosRequest request)
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                var response = _agendamentoService.Listar(codigoParceiro, request);

                return Ok(response);
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

        [HttpGet("Home")]
        [Authorize]
        public IActionResult ListarHome([FromQuery] ListarRequest request)
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                var response = _agendamentoService.ListarHome(request, codigoParceiro);

                return Ok(response);
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

        [HttpGet("Operacional")]
        [Authorize]
        public IActionResult ObterAgendamentoOperacional([FromQuery] AgendamentoOperacionalRequest request)
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                var response = _agendamentoService.ObterOperacional(request);

                return Ok(response);
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

        [HttpGet("{codigoAgendamento}")]
        [Authorize]
        public IActionResult Obter(Guid codigoAgendamento)
        {
            try
            {
                var response = _agendamentoService.Obter(codigoAgendamento);

                return Ok(response);
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

        [HttpPatch("Detalhado")]
        [Authorize]
        public IActionResult AtualizarDetalhado(AtualizarDetalhadoRequest request)
        {
            try
            {
                _agendamentoService.AtualizarDetalhado(request);

                return Ok();
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

        [HttpPost("Documento")]
        [Authorize]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentoRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var response = await _documentoService.InserirDocumento(request, codigoUsuario, Domain.Enums.TipoEntidadeUpload.Agendamento, Domain.Enums.TipoDocumento.Documento, ehAgendamento: true);

                return Ok(response);
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

        [HttpGet("DetalhadoAtendimento")]
        [Authorize]
        public IActionResult ListarDetalhadoAtendimento([FromQuery] DetalhadoAtendimentoRequest request)
        {
            try
            {
                var response = _agendamentoService.ListarDetalhadoAtendimentos(request);

                return Ok(response);
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
