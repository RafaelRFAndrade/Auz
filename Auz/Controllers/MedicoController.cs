using Amazon.S3;
using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
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
        private readonly IDocumentoService _documentoService;

        public MedicoController(ILogger<MedicoController> logger,
            IMedicoService medicoService,
            IDocumentoService documentoService)
        {
            _logger = logger;
            _medicoService = medicoService;
            _documentoService = documentoService;
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
        public ActionResult<ListarMedicosResponse> Listar([FromQuery] ListarRequest request)
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

        [Authorize]
        [HttpGet("{codigoMedico}")]
        public ActionResult Obter(Guid codigoMedico)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var response = _medicoService.Obter(codigoMedico, codigoUsuario);

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
        public IActionResult Atualizar(AtualizarMedicoRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _medicoService.Atualizar(request, codigoUsuario);

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
        [HttpPut("Completo")]
        public IActionResult AtualizarCompleto(AtualizarCompletoRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _medicoService.AtualizarCompleto(request, codigoUsuario);

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
        public IActionResult Desativar(DesativarMedicoRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                _medicoService.Desativar(request, codigoUsuario);

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

                var response = _medicoService.ObterPorDocumento(cpf, codigoParceiro);

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

                var response = _medicoService.BuscarDocumentos(cpf, codigoParceiro);

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
        [HttpGet("Detalhado")]
        public ActionResult ObterDetalhado([FromQuery] Guid codigoMedico)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var response = _medicoService.ObterMedicoDetalhado(codigoMedico);

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

        [HttpPost("FotoDePerfil")]
        [Authorize]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentoRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var response = await _documentoService.InserirDocumento(request, codigoUsuario, Domain.Enums.TipoEntidadeUpload.Perfil, Domain.Enums.TipoDocumento.FotoPerfil);

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

        [HttpGet("FotoDePerfil")]
        [Authorize]
        public async Task<IActionResult> GetDocumento([FromQuery] Guid codigoOperador)
        {
            try
            {
                var dadosDocumento = await _documentoService.ObterFotoPerfil(codigoOperador);

                if (dadosDocumento.Documento is null)
                    return NotFound();

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
                _logger.LogError(ex.Message);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Ocorreu um erro na requisição." });
            }
        }
    }
}
