using Application.Messaging.Request;
using Application.Messaging.Request.Agendamento;
using Application.Messaging.Response.Agendamento;
using Domain.Entidades;

namespace Application.Interfaces
{
    public interface IAgendamentoService
    {
        void Cadastrar(CadastroAgendamentoRequest request, Guid codigoUsuario);
        AgendamentosResponse Listar(Guid codigoParceiro, AgendamentosRequest request);
        AgendamentoOperacionalResponse ObterOperacional(AgendamentoOperacionalRequest request);
        AgendamentosHomeResponse ListarHome(ListarRequest request, Guid codigoParceiro);
        Agendamento Obter(Guid codigoAgendamento);
        void AtualizarDetalhado(AtualizarDetalhadoRequest request);
        DetalhadoAtendimentoResponse ListarDetalhadoAtendimentos(DetalhadoAtendimentoRequest request);
    }
}
