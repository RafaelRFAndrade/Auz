using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Parceiro;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Base;

namespace Web.Controllers
{
    public class ParceiroController : AuzControllerBase
    {
        private readonly ILogger<ParceiroController> _logger;
        private readonly IParceiroService _parceiroService;

        public ParceiroController(ILogger<ParceiroController> logger,
            IParceiroService parceiroService)
        {
            _logger = logger;
            _parceiroService = parceiroService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Obter()
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                var parceiro = _parceiroService.Obter(codigoParceiro);

                return Ok(parceiro);
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
        [HttpPatch]
        public IActionResult Atualizar(AtualizarParceiroRequest request)
        {
            try
            {
                var codigoParceiro = ObterCodigoParceiro();

                _parceiroService.Atualizar(request, codigoParceiro);

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
    }
}
