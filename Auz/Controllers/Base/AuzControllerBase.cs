using Application.Messaging.Exception;
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
    }
}
