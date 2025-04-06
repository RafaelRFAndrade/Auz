using Application.Messaging.Request;
using Application.Messaging.Request.Medico;
using Application.Messaging.Response.Medico;
using Domain.Entidades;

namespace Application.Interfaces
{
    public interface IMedicoService
    {
        void Cadastrar(CadastroMedicoRequest request, Guid codigoUsuario);
        ListarMedicosResponse Listar(ListarRequest request, Guid codigoUsuario);
        void Atualizar(AtualizarMedicoRequest request, Guid codigoUsuario);
        void Desativar(DesativarMedicoRequest request, Guid codigoUsuario);
        Medico Obter(Guid codigoMedico, Guid codigoUsuario);
    }
}
