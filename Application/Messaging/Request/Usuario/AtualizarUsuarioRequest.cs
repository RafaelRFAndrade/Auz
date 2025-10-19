using Application.Messaging.Exception;
using Domain.Enums;

namespace Application.Messaging.Request.Usuario
{
    public class AtualizarUsuarioRequest
    {
        public Guid Codigo { get; set; }

        public Situacao Situacao { get; set; }

        public string Nome { get; set; }

        public string Email { get; set; }

        public string Senha { get; set; }

        public TipoPermissao TipoPermissao { get; set; }

        public void Validar()
        {
            if (Codigo == Guid.Empty)
                throw new AuzException("Codigo ausente");
        }
    }
}
