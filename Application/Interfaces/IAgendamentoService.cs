using Application.Messaging.Request.Agendamento;
using Application.Messaging.Response.Agendamento;
using Infra.RawQueryResult;

namespace Application.Interfaces
{
    public interface IAgendamentoService
    {
        void Cadastrar(CadastroAgendamentoRequest request, Guid codigoUsuario);
        List<AgendamentoRawQueryResult> Listar(Guid codigoParceiro);
    }
}
