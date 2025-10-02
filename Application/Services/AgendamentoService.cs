using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Agendamento;
using Application.Messaging.Response.Agendamento;
using Domain.Entidades;
using Infra.RawQueryResult;
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

            var agendamento = new Agendamento
            {
                Codigo = Guid.NewGuid(),
                CodigoAtendimento = request.CodigoAtendimento,
                Situacao = Domain.Enums.Situacao.Ativo,
                DtInclusao = DateTime.Now,
                Descricao = request.Descricao,
                DtAgendamento = request.DtAgendamento,
                CodigoUsuario = codigoUsuario
            };

            if (_agendamentoRepository.VerificarDisponibilidade(agendamento.CodigoAtendimento, agendamento.DtAgendamento))
                throw new AuzException("O operador já possuí um agendamento proximo a data marcada.");

            _agendamentoRepository.Inserir(agendamento);
        }

        public AgendamentosResponse Listar(Guid codigoParceiro, AgendamentosRequest request)
        {
            var diaInicial = new DateTime(request.Ano, request.Mes, 01); 

            var agendamentos = _agendamentoRepository.ObterAgendamentosPorParceiro(codigoParceiro, diaInicial);

            var qtdAgendamentos = _agendamentoRepository.ObterQtdAgendamentosPorParceiro(codigoParceiro, diaInicial);

            return new AgendamentosResponse
            {
                Agendamentos = agendamentos,
                QtdAgendamentos = qtdAgendamentos.Count,
                Sucesso = true
            };
        }
    }
}