using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Paciente;
using Application.Services;
using AutoMapper;
using Domain.Entidades;
using FluentAssertions;
using Infra.RawQueryResult;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Pacientes;
using Moq;

namespace Application.Tests.Services
{
    public class PacienteServiceTests
    {
        private readonly Mock<IPacienteRepository> _pacienteRepoMock;
        private readonly Mock<IAtendimentoRepository> _atendimentoRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PacienteService _service;

        public PacienteServiceTests()
        {
            _pacienteRepoMock = new Mock<IPacienteRepository>();
            _atendimentoRepoMock = new Mock<IAtendimentoRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new PacienteService(
                _pacienteRepoMock.Object,
                _mapperMock.Object,
                _atendimentoRepoMock.Object
            );
        }

        [Fact]
        public void Cadastrar_DeveInserirPaciente()
        {
            // Arrange
            var request = new CadastroPacienteRequest
            {
                Nome = "Paciente Teste",
                DocumentoFederal = "210.576.230-16",
                Email = "paciente@teste.com",
                Telefone = "99999999"
            };
            var codigoUsuario = Guid.NewGuid();

            // Act
            _service.Cadastrar(request, codigoUsuario);

            // Assert
            _pacienteRepoMock.Verify(r => r.Inserir(It.Is<Paciente>(
                p => p.Nome == request.Nome &&
                     p.CodigoUsuario == codigoUsuario &&
                     p.DocumentoFederal == request.DocumentoFederal
            )), Times.Once);
        }

        [Fact]
        public void Listar_DeveRetornarListaComTotalPaginas()
        {
            // Arrange
            var request = new ListarRequest { Filtro = "teste", Pagina = 1, ItensPorPagina = 10 };
            var codigoUsuario = Guid.NewGuid();

            _pacienteRepoMock.Setup(r => r.Listar(It.IsAny<string>(), codigoUsuario, 1, 10))
                .Returns(new List<ListarPacientesRawQuery> { new ListarPacientesRawQuery { Nome = "Paciente1" } });

            _pacienteRepoMock.Setup(r => r.ObterTotalizador(It.IsAny<string>(), codigoUsuario))
                .Returns(new CountRawQuery { Count = 1 });

            // Act
            var response = _service.Listar(request, codigoUsuario);

            // Assert
            response.ListaPacientes.Should().HaveCount(1);
            response.Itens.Should().Be(1);
            response.TotalPaginas.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Atualizar_ComDocumentoFederalDiferente_DeveLancarExcecao()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var paciente = new Paciente
            {
                CodigoUsuario = codigoUsuario,
                DocumentoFederal = "210.576.230-16",
                Nome = "Teste",
                Telefone = "89968484388",
                Email = "Teste@gmail.com"
            };

            _pacienteRepoMock.Setup(r => r.Obter(It.IsAny<Guid>())).Returns(paciente);

            var request = new AtualizarPacienteRequest
            {
                CodigoPaciente = Guid.NewGuid(),
                DocumentoFederal = "678.397.600-90",
                Nome = "Teste",
                Telefone = "89968484388",
                Email = "Teste@gmail.com"
            };

            // Act
            Action act = () => _service.Atualizar(request, codigoUsuario);

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Não é possível alterar o documento federal.");
        }

        [Fact]
        public void Desativar_ComAtendimentosAtivos_DeveLancarExcecao()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var paciente = new Paciente
            {
                Codigo = Guid.NewGuid(),
                CodigoUsuario = codigoUsuario
            };

            _pacienteRepoMock.Setup(r => r.Obter(It.IsAny<Guid>())).Returns(paciente);
            _atendimentoRepoMock.Setup(r => r.ValidarAtendimentoAtivosPorPaciente(paciente.Codigo)).Returns(true);

            var request = new DesativarPacienteRequest { CodigoPaciente = paciente.Codigo };

            // Act
            Action act = () => _service.Desativar(request, codigoUsuario);

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Paciente possuí atendimentos em andamento");
        }

        [Fact]
        public void Obter_ComUsuarioDiferente_DeveLancarExcecao()
        {
            // Arrange
            var paciente = new Paciente { CodigoUsuario = Guid.NewGuid() };
            _pacienteRepoMock.Setup(r => r.Obter(It.IsAny<Guid>())).Returns(paciente);

            // Act
            Action act = () => _service.Obter(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Usuário não tem permissão para vizualizar esse médico");
        }

        [Fact]
        public void ObterPorDocumentoFederal_DeveChamarRepositorio()
        {
            // Arrange
            var paciente = new Paciente { DocumentoFederal = "210.576.230-16" };
            _pacienteRepoMock.Setup(r => r.ObterPorDocumentoFederal("210.576.230-16", It.IsAny<Guid>())).Returns(paciente);

            // Act
            var result = _service.ObterPorDocumentoFederal("210.576.230-16", Guid.NewGuid());

            // Assert
            result.Should().Be(paciente);
        }

        [Fact]
        public void BuscarDocumentos_DeveRetornarLista()
        {
            // Arrange
            var documentos = new List<ListarDocumentosRawQuery> { new ListarDocumentosRawQuery() };
            _pacienteRepoMock.Setup(r => r.ObterDocumentos("210.576.230-16", It.IsAny<Guid>())).Returns(documentos);

            // Act
            var result = _service.BuscarDocumentos("210.576.230-16", Guid.NewGuid());

            // Assert
            result.Should().HaveCount(1);
        }
    }
}
