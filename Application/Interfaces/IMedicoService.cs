using Application.Messaging.Request.Medico;

namespace Application.Interfaces
{
    public interface IMedicoService
    {
        void Cadastrar(CadastroMedicoRequest request, Guid codigoUsuario);
    }
}
