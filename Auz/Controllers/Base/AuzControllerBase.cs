using Application.Messaging.Exception;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers.Base
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuzControllerBase : ControllerBase
    {
        protected Guid ObterCodigoUsuario()
        {
            var codigoUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (codigoUsuario is null || string.IsNullOrWhiteSpace(codigoUsuario))
                throw new AuzException("Usuário não encontrado.");

            return Guid.Parse(codigoUsuario);
        }

        protected Guid ObterCodigoParceiro()
        {
            var codigoParceiro = User.FindFirstValue("ParceiroId");

            if (string.IsNullOrWhiteSpace(codigoParceiro))
                throw new AuzException("Parceiro não encontrado.");

            return Guid.Parse(codigoParceiro);
        }

        protected TipoPermissao ObterPermissao()
        {
            var permissaoClaim = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrWhiteSpace(permissaoClaim))
                throw new AuzException("Permissão do usuário não encontrada.");

            if (!Enum.TryParse<TipoPermissao>(permissaoClaim, out var permissao))
                throw new AuzException($"Permissão '{permissaoClaim}' inválida.");

            return permissao;
        }
    }
}
