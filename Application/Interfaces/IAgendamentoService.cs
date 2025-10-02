using Application.Messaging.Request.Agendamento;
using Application.Messaging.Response.Agendamento;

namespace Application.Interfaces
{
    public interface IAgendamentoService
    {
        void Cadastrar(CadastroAgendamentoRequest request, Guid codigoUsuario);
        AgendamentosResponse Listar(Guid codigoParceiro, AgendamentosRequest request);
    }
}
