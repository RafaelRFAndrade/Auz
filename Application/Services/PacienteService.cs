using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Paciente;
using Application.Messaging.Response.Paciente;
using AutoMapper;
using Domain.Entidades;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Pacientes;

namespace Application.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IMapper _mapper;
        private readonly IAtendimentoRepository _atendimentoRepository;

        public PacienteService(IPacienteRepository pacienteRepository, IMapper mapper, IAtendimentoRepository atendimentoRepository)
        {
            _pacienteRepository = pacienteRepository;
            _mapper = mapper;
            _atendimentoRepository = atendimentoRepository;
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

        public ListarPacienteResponse Listar(ListarRequest request, Guid codigoUsuario)
        {
            var listarPacientes = _pacienteRepository.Listar(request.Filtro, codigoUsuario, request.Pagina.GetValueOrDefault(), request.ItensPorPagina.GetValueOrDefault());

            var totalizador = _pacienteRepository.ObterTotalizador(request.Filtro, codigoUsuario);

            var total = totalizador.Count / request.ItensPorPagina.GetValueOrDefault(25);

            return new ListarPacienteResponse
            {
                ListaPacientes = listarPacientes,
                TotalPaginas = total == 0 ? 25 : total,
                Itens = totalizador.Count
            };
        }

        public void Atualizar(AtualizarPacienteRequest request, Guid codigoUsuario)
        {
            request.Validar();

            var paciente = _pacienteRepository.Obter(request.CodigoPaciente) ??
                throw new AuzException("Paciente não encontrado");

            if (paciente.CodigoUsuario != codigoUsuario)
                throw new AuzException("Usuário não possuí permissão para alterar o paciente");

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

            if (paciente.CodigoUsuario != codigoUsuario)
                throw new AuzException("Usuário não possuí permissão para alterar o paciente");

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
    }
}
