using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(ILogger<UsuarioController> logger, IUsuarioService usuarioService)
        {
            _logger = logger;
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            try
            {

                return Ok();
            }
            catch (AuzException ex)
            {
                return BadRequest(new { Sucesso = false, Mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Fudeu." });
            }
        }

        [HttpPost]
        public IActionResult Cadastrar(CadastroUsuarioRequest cadastroUsuarioRequest)
        {
            try
            {
                _usuarioService.Cadastrar(cadastroUsuarioRequest);

                return Created();
            }
            catch (AuzException ex)
            {
                return BadRequest(new { Sucesso = false, Mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Fudeu." });
            }
        }
    }
}
