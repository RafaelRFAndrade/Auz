using Domain.Enums;

namespace Application.Messaging.Request
{
    public class CadastroUsuarioRequest
    {
        public string Nome {  get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public TipoPermissao TipoPermissao { get; set; }
    }
}
