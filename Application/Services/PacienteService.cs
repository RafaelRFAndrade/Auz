using Application.Interfaces;
using Application.Messaging.Request.Paciente;
using Domain.Entidades;
using Infra.Repositories.Pacientes;

namespace Application.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _pacienteRepository;

        public PacienteService(IPacienteRepository pacienteRepository)
        {
            _pacienteRepository = pacienteRepository;
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
    }
}
