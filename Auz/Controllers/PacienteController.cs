using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Paciente;
using Application.Messaging.Response.Paciente;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PacienteController : AuzControllerBase
    {
        private readonly ILogger<PacienteController> _logger;
        private readonly IPacienteService _pacienteService;

        public PacienteController(ILogger<PacienteController> logger, IPacienteService pacienteService)
        {
            _logger = logger;
            _pacienteService = pacienteService;
        }

        [Authorize]
        [HttpPost]
        public IActionResult Cadastrar(CadastroPacienteRequest cadastroPacienteRequest)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _pacienteService.Cadastrar(cadastroPacienteRequest, codigoUsuario);

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
        public ActionResult<ListarPacienteResponse> Listar([FromQuery] ListarRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var response = _pacienteService.Listar(request, codigoUsuario);

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

        [Authorize]
        [HttpPut]
        public IActionResult Atualizar(AtualizarPacienteRequest cadastroPacienteRequest)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _pacienteService.Atualizar(cadastroPacienteRequest, codigoUsuario);

                return Ok();
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
        [HttpDelete]
        public IActionResult Desativar(DesativarPacienteRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _pacienteService.Desativar(request, codigoUsuario);

                return Ok();
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
        [HttpGet("{codigoPaciente}")]
        public ActionResult Obter(Guid codigoPaciente)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var response = _pacienteService.Obter(codigoPaciente, codigoUsuario);

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

        [Authorize]
        [HttpPut("Detalhado")]
        public IActionResult AtualizarDetalhado(AtualizarPacienteDetalhadoRequest atualizarPacienteDetalhadoRequest)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _pacienteService.AtualizarDetalhado(atualizarPacienteDetalhadoRequest, codigoUsuario);

                return Ok();
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
        [HttpGet("BuscarPorCpf/{cpf}")]
        public ActionResult ObterPorDocumento(string cpf)
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                var response = _pacienteService.ObterPorDocumentoFederal(cpf, codigoParceiro);

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

        [Authorize]
        [HttpGet("BuscarDocumentos/{cpf}")]
        public ActionResult BuscarDocumentos(string cpf)
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                var response = _pacienteService.BuscarDocumentos(cpf, codigoParceiro);

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
