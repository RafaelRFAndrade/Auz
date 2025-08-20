using Application.Interfaces;
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

            _agendamentoRepository.Inserir(agendamento);
        }

        public List<AgendamentoRawQueryResult> Listar(Guid codigoParceiro)
        {
            return _agendamentoRepository.ObterAgendamentosPorParceiro(codigoParceiro);

            //var totalizador = _agendamentoRepository.Totalizar(codigoParceiro);

            //var total = totalizador.Count / request.ItensPorPagina.GetValueOrDefault(25);

            //return new AgendamentosResponse
            //{
            //    Agendamentos = agendamentos,
            //    TotalPaginas = total == 0 ? 1 : total,
            //    Itens = totalizador.Count
            //};
        }
    }
}