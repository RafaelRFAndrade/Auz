using Application.Messaging.Request;
using Domain.Entidades;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        void Cadastrar(CadastroUsuarioRequest request);
        Usuario Login(Messaging.Request.LoginRequest request);
    }
}
