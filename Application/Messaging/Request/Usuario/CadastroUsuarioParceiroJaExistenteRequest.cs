using Application.Messaging.Exception;
using Domain.Enums;

namespace Application.Messaging.Request.Usuario
{
    public class CadastroUsuarioParceiroJaExistenteRequest
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public TipoPermissao TipoPermissao { get; set; }

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                throw new AuzException("Nome ausente");

            if (string.IsNullOrWhiteSpace(Senha))
                throw new AuzException("Senha ausente");

            if (Senha?.Length < 6)
                throw new AuzException("Senha precisa de pelo menos 6 caracteres");

            if (string.IsNullOrWhiteSpace(Email))
                throw new AuzException("Email ausente");
        }
    }
}
