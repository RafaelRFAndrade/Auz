using Application.Interfaces;
using Application.Messaging.Request.Agendamento;
using Domain.Entidades;
using Infra.Repositories.Agendamentos;

namespace Application.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly IAgendamentoRepository _agendamentoRepository;

        public AgendamentoService(IAgendamentoRepository agendamentoRepository)
        {
            _agendamentoRepository = agendamentoRepository;
        }

        public void Cadastrar(CadastroAgendamentoRequest request, Guid codigoUsuario)
        {
            request.Validar();

            var atendimento = new Agendamento
            {
                Codigo = Guid.NewGuid(),
                CodigoAtendimento = request.CodigoAtendimento,
                Situacao = Domain.Enums.Situacao.Ativo,
                DtInclusao = DateTime.Now,
                Descricao = request.Descricao,
                DtAgendamento = request.DtAgendamento,
                CodigoUsuario = codigoUsuario
            };

            _agendamentoRepository.Inserir(atendimento);
        }
    }
}