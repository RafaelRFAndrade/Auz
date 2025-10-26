using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Agendamento;
using Application.Messaging.Response.Agendamento;
using AutoMapper;
using Domain.Entidades;
using Infra.Repositories.Agendamentos;
using Infra.Repositories.MedicoUsuarioOperacional;
using Microsoft.VisualBasic;

namespace Application.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IMedicoUsuarioOperacionalRepository _medicoUsuarioOperacionalRepository;
        private readonly IMapper _mapper;

        public AgendamentoService(IAgendamentoRepository agendamentoRepository,
            IMedicoUsuarioOperacionalRepository medicoUsuarioOperacionalRepository,
            IMapper mapper)
        {
            _agendamentoRepository = agendamentoRepository;
            _medicoUsuarioOperacionalRepository = medicoUsuarioOperacionalRepository;
            _mapper = mapper;
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

        public AgendamentoOperacionalResponse ObterOperacional(AgendamentoOperacionalRequest request)
        {
            var medicoOperacional = _medicoUsuarioOperacionalRepository.Obter(request.CodigoMedicoUsuarioOperacional) ?? 
                    throw new AuzException("Relacionamento não encontrado.");

            if (!medicoOperacional.Ativo)
                throw new AuzException("Relacionamento desativado.");

            var pagina = Math.Max(1, request.Pagina.GetValueOrDefault(1));

            var itensPorPagina = request.ItensPorPagina.GetValueOrDefault(10);
            if (itensPorPagina <= 0) itensPorPagina = 10;

            var agendamentosOperacionais = _agendamentoRepository.ObterOperacional(medicoOperacional.CodigoUsuario, medicoOperacional.CodigoMedico, pagina, itensPorPagina);

            var totalizador = _agendamentoRepository.ObterTotalizadorOperacional(medicoOperacional.CodigoUsuario, medicoOperacional.CodigoMedico);
            var totalItens = totalizador?.Count ?? 0;

            var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
            totalPaginas = Math.Max(1, totalPaginas);

            return new AgendamentoOperacionalResponse
            {
                AgendamentoOperacionais = agendamentosOperacionais,
                TotalPaginas = totalPaginas,
                Itens = totalItens
            };
        }

        public AgendamentosHomeResponse ListarHome(ListarRequest request, Guid codigoParceiro)
        {
            var pagina = Math.Max(1, request.Pagina.GetValueOrDefault(1));

            var itensPorPagina = request.ItensPorPagina.GetValueOrDefault(10);
            if (itensPorPagina <= 0) itensPorPagina = 10;

            var agendamentos = _agendamentoRepository.ObterAgendamentosPorParceiro(codigoParceiro, pagina, itensPorPagina);

            var totalizador = _agendamentoRepository.ObterTotalizadorAgendamentosPorParceiro(codigoParceiro);
            var totalItens = totalizador?.Count ?? 0;

            var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
            totalPaginas = Math.Max(1, totalPaginas);

            return new AgendamentosHomeResponse
            {
                Agendamentos = agendamentos,
                TotalPaginas = totalPaginas,
                Itens = totalItens
            };
        }

        public Agendamento Obter(Guid codigoAgendamento)
        {
            var agendamento = _agendamentoRepository.Obter(codigoAgendamento)
                ?? throw new AuzException("Agendamento não encontrado.");

            return agendamento;
        }

        public void AtualizarDetalhado(AtualizarDetalhadoRequest request)
        {
            var agendamento = _agendamentoRepository.Obter(request.Codigo)
                ?? throw new AuzException("Agendamento não encontrado.");

            _mapper.Map(request, agendamento);  

            agendamento.DtSituacao = DateTime.Now;

            _agendamentoRepository.Atualizar(agendamento);
        }

        public DetalhadoAtendimentoResponse ListarDetalhadoAtendimentos(DetalhadoAtendimentoRequest request)
        {
            var pagina = Math.Max(1, request.Pagina.GetValueOrDefault(1));

            var itensPorPagina = request.ItensPorPagina.GetValueOrDefault(10);
            if (itensPorPagina <= 0) itensPorPagina = 10;

            var agendamentos = _agendamentoRepository.ObterAgendamentosPorAtendimento(request.CodigoAtendimento, pagina, itensPorPagina);

            var totalizador = _agendamentoRepository.ObterTotalizadorAgendamentosPorAtendimento(request.CodigoAtendimento);
            var totalItens = totalizador?.Count ?? 0;

            var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
            totalPaginas = Math.Max(1, totalPaginas);

            return new DetalhadoAtendimentoResponse
            {
                Agendamentos = agendamentos,
                TotalPaginas = totalPaginas,
                Itens = totalItens
            };
        }
    }
}