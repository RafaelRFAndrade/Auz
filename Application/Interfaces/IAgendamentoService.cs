using Application.Messaging.Request.Agendamento;

namespace Application.Interfaces
{
    public interface IAgendamentoService
    {
        void Cadastrar(CadastroAgendamentoRequest request, Guid codigoUsuario);
    }
}
