using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Paciente;
using Application.Messaging.Response.Paciente;
using AutoMapper;
using Domain.Entidades;
using Infra.RawQueryResult;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.MedicoUsuarioOperacional;
using Infra.Repositories.Pacientes;

namespace Application.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IMapper _mapper;
        private readonly IAtendimentoRepository _atendimentoRepository;
        private readonly IMedicoUsuarioOperacionalRepository _medicoUsuarioOperacionalRepository;

        public PacienteService(IPacienteRepository pacienteRepository,
            IMapper mapper, 
            IAtendimentoRepository atendimentoRepository,
            IMedicoUsuarioOperacionalRepository medicoUsuarioOperacionalRepository)
        {
            _pacienteRepository = pacienteRepository;
            _mapper = mapper;
            _atendimentoRepository = atendimentoRepository;
            _medicoUsuarioOperacionalRepository = medicoUsuarioOperacionalRepository;
        }

        public void Cadastrar(CadastroPacienteRequest request, Guid codigoUsuario)
        {
            request.Validar();

            var paciente = new Paciente
            {
                Codigo = Guid.NewGuid(),
                CodigoUsuario = codigoUsuario,
                Nome = request.Nome,
                Situacao = Domain.Enums.Situacao.Ativo,
                DtInclusao = DateTime.Now,
                DocumentoFederal = request.DocumentoFederal,
                Telefone = request.Telefone,
                Email = request.Email,
            };

            _pacienteRepository.Inserir(paciente);
        }

        public ListarPacienteResponse Listar(ListarRequest request, Guid codigoParceiro)
        {
            var pagina = Math.Max(1, request.Pagina.GetValueOrDefault(1));

            var itensPorPagina = request.ItensPorPagina.GetValueOrDefault(25);
            if (itensPorPagina <= 0) itensPorPagina = 25;

            var listarPacientes = _pacienteRepository.Listar(request.Filtro, codigoParceiro, pagina, itensPorPagina);

            var totalizador = _pacienteRepository.ObterTotalizador(request.Filtro, codigoParceiro);

            var totalItens = totalizador?.Count ?? 0;

            var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
            totalPaginas = Math.Max(1, totalPaginas);

            return new ListarPacienteResponse
            {
                ListaPacientes = listarPacientes,
                TotalPaginas = totalPaginas,
                Itens = totalItens
            };
        }

        public void Atualizar(AtualizarPacienteRequest request, Guid codigoUsuario)
        {
            request.Validar();

            var paciente = _pacienteRepository.Obter(request.CodigoPaciente) ??
                throw new AuzException("Paciente não encontrado");

            if (paciente.DocumentoFederal != request.DocumentoFederal)
                throw new AuzException("Não é possível alterar o documento federal.");

            _mapper.Map(request, paciente);

            paciente.DtSituacao = DateTime.Now;

            _pacienteRepository.Atualizar(paciente);
        }

        public void AtualizarDetalhado(AtualizarPacienteDetalhadoRequest request, Guid codigoUsuario)
        {
            request.Validar();

            var paciente = _pacienteRepository.Obter(request.CodigoPaciente) ??
                throw new AuzException("Paciente não encontrado");

            if (paciente.DocumentoFederal != request.DocumentoFederal)
                throw new AuzException("Não é possível alterar o documento federal.");

            _mapper.Map(request, paciente);

            paciente.DtSituacao = DateTime.Now;

            _pacienteRepository.Atualizar(paciente);
        }

        public void Desativar(DesativarPacienteRequest request, Guid codigoUsuario)
        {
            var paciente = _pacienteRepository.Obter(request.CodigoPaciente) ??
                throw new AuzException("Paciente não encontrado");

            if (_atendimentoRepository.ValidarAtendimentoAtivosPorPaciente(paciente.Codigo))
                throw new AuzException("Paciente possuí atendimentos em andamento");

            paciente.Situacao = Domain.Enums.Situacao.Desativo;
            paciente.DtSituacao = DateTime.Now;

            _pacienteRepository.Atualizar(paciente);
        }

        public Paciente Obter(Guid codigoPaciente, Guid codigoUsuario)
        {
            var paciente = _pacienteRepository.Obter(codigoPaciente);

            if (paciente.CodigoUsuario != codigoUsuario)
                throw new AuzException("Usuário não tem permissão para vizualizar esse médico");

            return paciente;
        }

        public Paciente ObterPorDocumentoFederal(string documentoFederal, Guid codigoParceiro)
        {
            var paciente = _pacienteRepository.ObterPorDocumentoFederal(documentoFederal, codigoParceiro);

            return paciente;
        }

        public List<ListarDocumentosRawQuery> BuscarDocumentos(string documentoFederal, Guid codigoParceiro)
        {
            var listaDocumentos = _pacienteRepository.ObterDocumentos(documentoFederal, codigoParceiro);

            return listaDocumentos;
        }

        public ListarPacienteResponse ObterOperacional(ObterOperacionalRequest request)
        {
            var medicoOperacional = _medicoUsuarioOperacionalRepository.Obter(request.CodigoMedicoUsuarioOperacional) ??
                 throw new AuzException("Relacionamento não encontrado.");

            if (!medicoOperacional.Ativo)
                throw new AuzException("Relacionamento desativado.");

            var pagina = Math.Max(1, request.Pagina.GetValueOrDefault(1));

            var itensPorPagina = request.ItensPorPagina.GetValueOrDefault(25);
            if (itensPorPagina <= 0) itensPorPagina = 25;

            var listarPacientes = _pacienteRepository.ObterOperacional(request.Filtro, medicoOperacional.CodigoUsuario, medicoOperacional.CodigoMedico, pagina, itensPorPagina);

            var totalizador = _pacienteRepository.ObterTotalizadorOperacional(request.Filtro, medicoOperacional.CodigoUsuario, medicoOperacional.CodigoMedico);

            var totalItens = totalizador?.Count ?? 0;

            var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
            totalPaginas = Math.Max(1, totalPaginas);

            return new ListarPacienteResponse
            {
                ListaPacientes = listarPacientes,
                TotalPaginas = totalPaginas,
                Itens = totalItens
            };
        }
    }
}
