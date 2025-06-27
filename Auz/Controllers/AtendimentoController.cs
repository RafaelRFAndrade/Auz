using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Atendimento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AtendimentoController : AuzControllerBase
    {
        private readonly ILogger<AtendimentoController> _logger;
        private readonly IAtendimentoService _atendimentoService;

        public AtendimentoController(ILogger<AtendimentoController> logger, IAtendimentoService atendimentoService)
        {
            _logger = logger;
            _atendimentoService = atendimentoService;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Cadastrar(CadastroAtendimentoRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _atendimentoService.Cadastrar(request, codigoUsuario);

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

        [HttpGet("Listar")]
        [Authorize]
        public IActionResult Listar([FromQuery]ListarAtendimentosRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var atendimentos = _atendimentoService.ListarAtendimentos(request, codigoUsuario);

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
