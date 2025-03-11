using Application.Messaging.Request;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        void Cadastrar(CadastroUsuarioRequest request);
    }
}
