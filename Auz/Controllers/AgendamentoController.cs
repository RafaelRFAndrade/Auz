using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Agendamento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AgendamentoController : AuzControllerBase
    {
        private readonly ILogger<AgendamentoController> _logger;
        private readonly IAgendamentoService _agendamentoService;

        public AgendamentoController(ILogger<AgendamentoController> logger, IAgendamentoService agendamentoService)
        {
            _logger = logger;
            _agendamentoService = agendamentoService;
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
                _logger.LogError(ex.Message);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Ocorreu um erro na requisição." });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Listar()
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                var response = _agendamentoService.Listar( codigoParceiro);

                return Ok(response);
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
