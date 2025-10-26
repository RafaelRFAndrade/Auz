using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Medico;
using Application.Services;
using AutoMapper;
using Domain.Entidades;
using FluentAssertions;
using Infra.RawQueryResult;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Medicos;
using Infra.Repositories.MedicoUsuarioOperacional;
using Moq;

namespace Application.Tests.Services
{
    public class MedicoServiceTests
    {
        private readonly Mock<IMedicoRepository> _medicoRepoMock;
        private readonly Mock<IAtendimentoRepository> _atendimentoRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly MedicoService _service;
        private readonly Mock<IMedicoUsuarioOperacionalRepository> _medicoUsuarioOperacionalRepoMock;
        public MedicoServiceTests()
        {
            _medicoRepoMock = new Mock<IMedicoRepository>();
            _atendimentoRepoMock = new Mock<IAtendimentoRepository>();
            _mapperMock = new Mock<IMapper>();
            _medicoUsuarioOperacionalRepoMock = new Mock<IMedicoUsuarioOperacionalRepository>();

            _service = new MedicoService(
                _medicoRepoMock.Object,
                _mapperMock.Object,
                _atendimentoRepoMock.Object,
                _medicoUsuarioOperacionalRepoMock.Object
            );
        }

        [Fact]
        public void Cadastrar_DeveInserirMedico()
        {
            // Arrange
            var request = new CadastroMedicoRequest
            {
                Nome = "Dr. Teste",
                CRM = "12345",
                Email = "teste@teste.com",
                Telefone = "89968484388",
                DocumentoFederal = "689.683.340-80"
            };

            var codigoUsuario = Guid.NewGuid();

            // Act
            _service.Cadastrar(request, codigoUsuario);

            // Assert
            _medicoRepoMock.Verify(r => r.Inserir(It.Is<Medico>(
                m => m.Nome == request.Nome &&
                     m.CodigoUsuario == codigoUsuario &&
                     m.CRM == request.CRM
            )), Times.Once);
        }

        [Fact]
        public void Listar_DeveRetornarListaComTotalPaginas()
        {
            // Arrange
            var request = new ListarRequest { Filtro = "teste", Pagina = 1, ItensPorPagina = 10 };
            var codigoUsuario = Guid.NewGuid();

            _medicoRepoMock.Setup(r => r.Listar(It.IsAny<string>(), codigoUsuario, 1, 10))
                .Returns(new List<ListarMedicoRawQuery> { new ListarMedicoRawQuery { Nome = "Medico1" } });

            _medicoRepoMock.Setup(r => r.ObterTotalizador(It.IsAny<string>(), codigoUsuario))
                .Returns(new CountRawQuery { Count = 3 } );

            // Act
            var response = _service.Listar(request, codigoUsuario);

            // Assert
            response.ListaMedicos.Should().HaveCount(1);
            response.Itens.Should().Be(3);
            response.TotalPaginas.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Obter_ComUsuarioDiferente_NaoDeveLancarExcecao()
        {
            // Arrange
            var medico = new Medico { CodigoUsuario = Guid.NewGuid() };
            _medicoRepoMock.Setup(r => r.Obter(It.IsAny<Guid>())).Returns(medico);

            // Act
            Action act = () => _service.Obter(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            act.Should().NotThrow<AuzException>();
        }

        [Fact]
        public void Atualizar_ComDocumentoFederalDiferente_DeveLancarExcecao()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var medico = new Medico
            {
                CodigoUsuario = codigoUsuario,
                DocumentoFederal = "689.683.340-80"
            };

            _medicoRepoMock.Setup(r => r.Obter(It.IsAny<Guid>())).Returns(medico);

            var request = new AtualizarMedicoRequest
            {
                Codigo = Guid.NewGuid(),
                DocumentoFederal = "226.362.150-42", // diferente do cadastrado
                Nome = "Teste",
                CRM = "20000SC",
                Email = "Teste@gmail.com",
                Telefone = "89968484388"
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
            var medico = new Medico
            {
                Codigo = Guid.NewGuid(),
                CodigoUsuario = codigoUsuario
            };

            _medicoRepoMock.Setup(r => r.Obter(It.IsAny<Guid>())).Returns(medico);
            _atendimentoRepoMock.Setup(r => r.ValidarAtendimentoAtivosPorMedico(medico.Codigo)).Returns(true);

            var request = new DesativarMedicoRequest { CodigoMedico = medico.Codigo };

            // Act
            Action act = () => _service.Desativar(request, codigoUsuario);

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Médico possuí atendimentos em andamento");
        }

        [Fact]
        public void ObterPorDocumento_DeveChamarRepositorio()
        {
            // Arrange
            var medico = new Medico { DocumentoFederal = "123" };
            _medicoRepoMock.Setup(r => r.ObterPorDocumentoFederal("123", It.IsAny<Guid>())).Returns(medico);

            // Act
            var result = _service.ObterPorDocumento("123", Guid.NewGuid());

            // Assert
            result.Should().Be(medico);
        }

        [Fact]
        public void BuscarDocumentos_DeveRetornarLista()
        {
            // Arrange
            var documentos = new List<ListarDocumentosRawQuery> { new ListarDocumentosRawQuery() };
            _medicoRepoMock.Setup(r => r.ObterDocumentos("123", It.IsAny<Guid>())).Returns(documentos);

            // Act
            var result = _service.BuscarDocumentos("123", Guid.NewGuid());

            // Assert
            result.Should().HaveCount(1);
        }
    }
}
