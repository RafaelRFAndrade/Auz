using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Medico;
using Application.Messaging.Response.Medico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MedicoController : AuzControllerBase
    {
        private readonly ILogger<MedicoController> _logger;
        private readonly IMedicoService _medicoService;

        public MedicoController(ILogger<MedicoController> logger, IMedicoService medicoService)
        {
            _logger = logger;
            _medicoService = medicoService;
        }

        [Authorize]
        [HttpPost]
        public IActionResult Cadastrar(CadastroMedicoRequest cadastroMedico)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _medicoService.Cadastrar(cadastroMedico, codigoUsuario);

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

        [Authorize]
        [HttpGet("Listar")]
        public ActionResult<ListarMedicosResponse> Listar(ListarMedicoRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var response = _medicoService.Listar(request, codigoUsuario);

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
