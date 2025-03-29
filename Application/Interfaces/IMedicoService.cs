using Application.Messaging.Request.Medico;
using Application.Messaging.Response.Medico;

namespace Application.Interfaces
{
    public interface IMedicoService
    {
        void Cadastrar(CadastroMedicoRequest request, Guid codigoUsuario);
        ListarMedicosResponse Listar(ListarMedicoRequest request, Guid codigoUsuario);
    }
}
