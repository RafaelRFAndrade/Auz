using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : AuzControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly IUsuarioService _usuarioService;
        private readonly IAutenticacaoService _autenticacaoService;

        public UsuarioController(ILogger<UsuarioController> logger,
            IUsuarioService usuarioService,
            IAutenticacaoService autenticacaoService)
        {
            _logger = logger;
            _usuarioService = usuarioService;
            _autenticacaoService = autenticacaoService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Obter()
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                return Ok(codigoUsuario);
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

        [HttpPost("Login")]
        public IActionResult Logar(Application.Messaging.Request.LoginRequest request)
        {
            try
            {
                var usuario = _usuarioService.Login(request);

                var token = _autenticacaoService.GenerateToken(usuario);

                return Ok(new { Token = token });
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
