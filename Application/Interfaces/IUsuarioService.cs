using Application.Messaging.Request.Usuario;
using Domain.Entidades;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        void Cadastrar(CadastroUsuarioRequest request);
        Usuario Login(LoginRequest request);
    }
}
