using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Usuario;
using Auz.Infrastructure.Logging;
using Domain.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : AuzControllerBase
    {
        private readonly ILokiLogger _logger;
        private readonly IUsuarioService _usuarioService;
        private readonly IAutenticacaoService _autenticacaoService;
        private readonly IDocumentoService _documentoService;

        public UsuarioController(ILokiLogger logger,
            IUsuarioService usuarioService,
            IAutenticacaoService autenticacaoService,
            IDocumentoService documentoService)
        {
            _logger = logger;
            _usuarioService = usuarioService;
            _autenticacaoService = autenticacaoService;
            _documentoService = documentoService;
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
                _logger.LogError($"Erro ao processar requisição: {ex.Message}", null, ex);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Ocorreu um erro na requisição." });
            }
        }

        [Authorize]
        [HttpGet("ObterUsuariosPorParceiro")]
        public IActionResult ObterUsuariosPorParceiro([FromQuery]ObterUsuariosPorParceiroRequest request)
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                var response = _usuarioService.ObterUsuariosPorParceiro(codigoParceiro, request);

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
                _logger.LogError($"Erro ao processar requisição: {ex.Message}", null, ex);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Ocorreu um erro na requisição." });
            }
        }

        [HttpPost("UsuarioPorParceiro")]
        [Authorize]
        public IActionResult CadastrarParceiroJaExistente(CadastroUsuarioParceiroJaExistenteRequest cadastroUsuarioRequest)
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                _usuarioService.CadastrarParceiroJaExistente(cadastroUsuarioRequest, codigoParceiro);

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


        [HttpDelete]
        public IActionResult Desativar(DesativarUsuarioRequest request)
        {
            try
            {
                _usuarioService.Desativar(request);

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

        [HttpPatch]
        public IActionResult Atualizar(AtualizarUsuarioRequest request)
        {
            try
            {
                _usuarioService.Atualizar(request);

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


        [HttpPost("Login")]
        public IActionResult Logar(LoginRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Tentativa de login com request nulo");
                    return BadRequest(new { Sucesso = false, Mensagem = "Dados de login não fornecidos." });
                }

                var usuario = _usuarioService.Login(request);

                if (usuario == null)
                {
                    _logger.LogWarning("Login retornou usuário nulo", new Dictionary<string, object> { ["email"] = request.Email });
                    return BadRequest(new { Sucesso = false, Mensagem = "Credenciais inválidas." });
                }

                var token = _autenticacaoService.GenerateToken(usuario);

                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogError("Token gerado está vazio ou nulo", new Dictionary<string, object> { ["usuarioId"] = usuario.Codigo.ToString() });
                    return StatusCode(500, new { Sucesso = false, Mensagem = "Erro ao gerar token de autenticação." });
                }

                return Ok(new { Token = token });
            }
            catch (AuzException ex)
            {
                return BadRequest(new { Sucesso = false, Mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar requisição de login: {ex.Message}", 
                    new Dictionary<string, object> 
                    { 
                        ["endpoint"] = "/Usuario/Login",
                        ["email"] = request?.Email ?? "não fornecido"
                    }, 
                    ex);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Ocorreu um erro na requisição." });
            }
        }

        [Authorize]
        [HttpGet("Home")]
        public IActionResult CarregarRelacionamentos()
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();
                var codigoParceiro = ObterCodigoParceiro(); 
                var response = _usuarioService.TrazerHome(codigoUsuario, codigoParceiro);

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

        [HttpPost("Documento")]
        [Authorize]
        public async Task<IActionResult> Upload([FromForm]UploadDocumentoRequest request)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var response = await _documentoService.InserirDocumento(request, codigoUsuario, Domain.Enums.TipoEntidadeUpload.Usuario, Domain.Enums.TipoDocumento.Documento);

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

        [HttpPost("Relacionar")]
        [Authorize]
        public IActionResult RelacionarMedicoUsuario(RelacionarMedicoUsuarioRequest relacionarMedicoUsuarioRequest)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var permissao = ObterPermissao();

                _usuarioService.RelacionarUsuarioMedico(relacionarMedicoUsuarioRequest, permissao, codigoUsuario);

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

        [HttpGet("Relacionamentos")]
        [Authorize]
        public IActionResult ObterUsuarioMedicoOperacional([FromQuery]ListarRequest listarRequest)
        {
            try
            {
                var codigoUsuario = ObterCodigoUsuario();

                var response = _usuarioService.ObterRelacionamentos(listarRequest, codigoUsuario);

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

        /// <summary>
        /// Endpoint de teste temporário para provocar NullReferenceException e testar o sistema de logging
        /// ATENÇÃO: Temporário!
        /// </summary>
        [HttpPost("TesteNullReference")]
        [Authorize]
        public IActionResult TesteNullReference()
        {
            try
            {
                _logger.LogInformation("Iniciando teste de NullReferenceException");

                // Simular um cenário onde um objeto pode ser null
                Usuario? usuario = null;
                
                // Forçar NullReferenceException para testar o logging
                var nome = usuario!.Nome; // NullReferenceException aqui

                return Ok(new { Mensagem = "Isso nunca será executado" });
            }
            catch (NullReferenceException ex)
            {
                // Log específico para NullReferenceException
                _logger.LogError("NullReferenceException capturada no teste", 
                    new Dictionary<string, object> 
                    { 
                        ["endpoint"] = "/Usuario/TesteNullReference",
                        ["tipoErro"] = "NullReferenceException",
                        ["mensagem"] = ex.Message,
                        ["stackTrace"] = ex.StackTrace ?? "N/A"
                    }, 
                    ex);
                
                return StatusCode(500, new { 
                    Sucesso = false, 
                    Mensagem = "NullReferenceException capturada e logada com sucesso no Loki",
                    Erro = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro inesperado no teste: {ex.Message}", null, ex);
                return StatusCode(500, new { Sucesso = false, Mensagem = "Ocorreu um erro na requisição." });
            }
        }
    }
}
