using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Atendimento;
using Application.Messaging.Response.Atendimento;
using AutoMapper;
using Domain.Entidades;
using Infra.Repositories.Agendamentos;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Documentos;
using Infra.Repositories.Medicos;
using Infra.Repositories.Pacientes;

namespace Application.Services
{
    public class AtendimentoService : IAtendimentoService
    {
        private readonly IAtendimentoRepository _atendimentoRepository;
        private readonly IMedicoRepository _medicoRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IDocumentoRepository _documentoRepository;
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IMapper _mapper;

        public AtendimentoService(IAtendimentoRepository atendimentoRepository,
            IMedicoRepository medicoRepository,
            IPacienteRepository pacienteRepository,
            IDocumentoRepository documentoRepository,
            IAgendamentoRepository agendamentoRepository,
            IMapper mapper)
        {
            _atendimentoRepository = atendimentoRepository;
            _medicoRepository = medicoRepository;
            _pacienteRepository = pacienteRepository;
            _documentoRepository = documentoRepository;
            _agendamentoRepository = agendamentoRepository;
            _mapper = mapper;
        }

        public void Cadastrar(CadastroAtendimentoRequest request, Guid codigoUsuario, Guid codigoParceiro)
        {
            request.Validar();

            var medico = _medicoRepository.ObterPorDocumentoFederal(request.DocumentoFederalMedico, codigoParceiro) ?? 
                throw new AuzException("Médico não encontrado.");

            var paciente = _pacienteRepository.ObterPorDocumentoFederal(request.DocumentoFederalPaciente, codigoParceiro);

            if (paciente is null && !request.CadastrarPaciente)
                throw new AuzException("Paciente não encontrado");

            if (request.CadastrarPaciente)
            {
                paciente = new Paciente
                {
                    Codigo = Guid.NewGuid(),
                    CodigoUsuario = codigoUsuario,
                    DocumentoFederal = request.DocumentoFederalPaciente,
                    Situacao = Domain.Enums.Situacao.Ativo,
                    DtInclusao = DateTime.Now,
                    DtSituacao = DateTime.Now,
                    Nome = request.NovoPaciente?.Nome,
                    Email = request.NovoPaciente?.Email,
                    Telefone = request.NovoPaciente?.Telefone
                };

                _pacienteRepository.Inserir(paciente);
            }
            
            var atendimento = new Atendimento
            {
                Codigo = Guid.NewGuid(),
                CodigoMedico = medico.Codigo,
                CodigoPaciente = paciente.Codigo,
                CodigoUsuario = codigoUsuario,
                Situacao = Domain.Enums.Situacao.Ativo,
                DtInclusao = DateTime.Now,
                Descricao = request.Descricao
            };

            _atendimentoRepository.Inserir(atendimento); 
        }

        public ListarAtendimentosResponse ListarAtendimentos(ListarAtendimentosRequest request, Guid codigoUsuario, Guid codigoParceiro)
        {
            var pagina = Math.Max(1, request.Pagina.GetValueOrDefault(1));

            var itensPorPagina = request.ItensPorPagina.GetValueOrDefault(25);
            if (itensPorPagina <= 0) itensPorPagina = 25;

            var atendimentos = _atendimentoRepository.ListarAtendimentos(codigoParceiro, pagina, itensPorPagina, request.Filtro);

            var totalizador = _atendimentoRepository.TotalizarAtendimentos(codigoParceiro, request.Filtro);

            var totalItens = totalizador?.Count ?? 0;

            var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
            totalPaginas = Math.Max(1, totalPaginas);

            return new ListarAtendimentosResponse
            {
                Atendimentos = atendimentos,
                TotalPaginas = totalPaginas,
                Itens = totalItens
            };
        }

        public void Deletar(Guid codigoAtendimento)
        {
            var atendimento = _atendimentoRepository.Obter(codigoAtendimento) ?? 
                throw new AuzException("Atendimento não encontrado.");

            atendimento.Situacao = Domain.Enums.Situacao.Desativo;
            atendimento.DtSituacao = DateTime.Now;

            _atendimentoRepository.Atualizar(atendimento);
        }

        public ObterAtendimentoResponse ObterRelacionamentos(Guid codigoAtendimento)
        {
            var atendimento = _atendimentoRepository.ObterAtendimento(codigoAtendimento);

            var agendamentos = _agendamentoRepository.ObterAgendamentosPorAtendimento(codigoAtendimento);

            var response = new ObterAtendimentoResponse();

            _mapper.Map(atendimento, response);

            response.Agendamentos = agendamentos;

            return response;
        }
    }
}
