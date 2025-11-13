using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Paciente;
using Application.Services;
using AutoMapper;
using Domain.Entidades;
using FluentAssertions;
using Infra.RawQueryResult;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.MedicoUsuarioOperacional;
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
        private readonly Mock<IMedicoUsuarioOperacionalRepository> _medicoUsuarioOperacionalRepository;

        public PacienteServiceTests()
        {
            _pacienteRepoMock = new Mock<IPacienteRepository>();
            _atendimentoRepoMock = new Mock<IAtendimentoRepository>();
            _mapperMock = new Mock<IMapper>();
            _medicoUsuarioOperacionalRepository = new Mock<IMedicoUsuarioOperacionalRepository>();

            _service = new PacienteService(
                _pacienteRepoMock.Object,
                _mapperMock.Object,
                _atendimentoRepoMock.Object,
                _medicoUsuarioOperacionalRepository.Object
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
            var codigoPaciente = Guid.NewGuid();
            var codigoUsuarioPaciente = Guid.NewGuid();
            var codigoUsuarioDiferente = Guid.NewGuid();
            
            var paciente = new Paciente 
            { 
                Codigo = codigoPaciente,
                CodigoUsuario = codigoUsuarioPaciente 
            };
            
            _pacienteRepoMock.Setup(r => r.Obter(codigoPaciente)).Returns(paciente);

            // Act
            Action act = () => _service.Obter(codigoPaciente, codigoUsuarioDiferente);

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Usuário não tem permissão para vizualizar esse paciente");
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

        [Fact]
        public void Obter_ComUsuarioCorreto_DeveRetornarPaciente()
        {
            // Arrange
            var codigoPaciente = Guid.NewGuid();
            var codigoUsuario = Guid.NewGuid();
            var paciente = new Paciente 
            { 
                Codigo = codigoPaciente,
                CodigoUsuario = codigoUsuario,
                Nome = "Paciente Teste"
            };
            
            _pacienteRepoMock.Setup(r => r.Obter(codigoPaciente)).Returns(paciente);

            // Act
            var result = _service.Obter(codigoPaciente, codigoUsuario);

            // Assert
            result.Should().NotBeNull();
            result.Codigo.Should().Be(codigoPaciente);
            result.Nome.Should().Be("Paciente Teste");
        }

        [Fact]
        public void Obter_ComPacienteNaoEncontrado_DeveLancarExcecao()
        {
            // Arrange
            var codigoPaciente = Guid.NewGuid();
            var codigoUsuario = Guid.NewGuid();
            
            _pacienteRepoMock.Setup(r => r.Obter(codigoPaciente)).Returns((Paciente)null);

            // Act
            Action act = () => _service.Obter(codigoPaciente, codigoUsuario);

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Paciente não encontrado");
        }

        [Fact]
        public void Atualizar_ComDadosValidos_DeveAtualizarPaciente()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var codigoPaciente = Guid.NewGuid();
            var paciente = new Paciente
            {
                Codigo = codigoPaciente,
                CodigoUsuario = codigoUsuario,
                DocumentoFederal = "210.576.230-16",
                Nome = "Nome Antigo"
            };

            _pacienteRepoMock.Setup(r => r.Obter(codigoPaciente)).Returns(paciente);

            var request = new AtualizarPacienteRequest
            {
                CodigoPaciente = codigoPaciente,
                DocumentoFederal = "210.576.230-16",
                Nome = "Nome Novo",
                Telefone = "89968484388",
                Email = "novo@email.com"
            };

            // Act
            _service.Atualizar(request, codigoUsuario);

            // Assert
            _pacienteRepoMock.Verify(r => r.Atualizar(It.IsAny<Paciente>()), Times.Once);
        }

        [Fact]
        public void Desativar_ComPacienteValido_DeveDesativarPaciente()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var codigoPaciente = Guid.NewGuid();
            var paciente = new Paciente
            {
                Codigo = codigoPaciente,
                CodigoUsuario = codigoUsuario
            };

            _pacienteRepoMock.Setup(r => r.Obter(codigoPaciente)).Returns(paciente);
            _atendimentoRepoMock.Setup(r => r.ValidarAtendimentoAtivosPorPaciente(codigoPaciente)).Returns(false);

            var request = new DesativarPacienteRequest { CodigoPaciente = codigoPaciente };

            // Act
            _service.Desativar(request, codigoUsuario);

            // Assert
            paciente.Situacao.Should().Be(Domain.Enums.Situacao.Desativo);
            _pacienteRepoMock.Verify(r => r.Atualizar(paciente), Times.Once);
        }

        [Fact]
        public void Listar_ComListaVazia_DeveRetornarListaVazia()
        {
            // Arrange
            var request = new ListarRequest { Filtro = "teste", Pagina = 1, ItensPorPagina = 10 };
            var codigoParceiro = Guid.NewGuid();

            _pacienteRepoMock.Setup(r => r.Listar(It.IsAny<string>(), codigoParceiro, 1, 10))
                .Returns(new List<ListarPacientesRawQuery>());

            _pacienteRepoMock.Setup(r => r.ObterTotalizador(It.IsAny<string>(), codigoParceiro))
                .Returns(new CountRawQuery { Count = 0 });

            // Act
            var response = _service.Listar(request, codigoParceiro);

            // Assert
            response.ListaPacientes.Should().BeEmpty();
            response.Itens.Should().Be(0);
            response.TotalPaginas.Should().Be(1);
        }

        [Fact]
        public void ObterPorDocumentoFederal_ComDocumentoInexistente_DeveRetornarNull()
        {
            // Arrange
            _pacienteRepoMock.Setup(r => r.ObterPorDocumentoFederal(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns((Paciente)null);

            // Act
            var result = _service.ObterPorDocumentoFederal("999.999.999-99", Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void BuscarDocumentos_ComListaVazia_DeveRetornarListaVazia()
        {
            // Arrange
            _pacienteRepoMock.Setup(r => r.ObterDocumentos(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(new List<ListarDocumentosRawQuery>());

            // Act
            var result = _service.BuscarDocumentos("210.576.230-16", Guid.NewGuid());

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void AtualizarDetalhado_ComDadosValidos_DeveAtualizarPaciente()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var codigoPaciente = Guid.NewGuid();
            var paciente = new Paciente
            {
                Codigo = codigoPaciente,
                CodigoUsuario = codigoUsuario,
                DocumentoFederal = "210.576.230-16"
            };

            _pacienteRepoMock.Setup(r => r.Obter(codigoPaciente)).Returns(paciente);

            var request = new AtualizarPacienteDetalhadoRequest
            {
                CodigoPaciente = codigoPaciente,
                DocumentoFederal = "210.576.230-16",
                Nome = "Nome Atualizado",
                Telefone = "89968484388",
                Email = "teste@email.com"
            };

            // Act
            _service.AtualizarDetalhado(request, codigoUsuario);

            // Assert
            _pacienteRepoMock.Verify(r => r.Atualizar(It.IsAny<Paciente>()), Times.Once);
        }

        [Fact]
        public void AtualizarDetalhado_ComPacienteNaoEncontrado_DeveLancarExcecao()
        {
            // Arrange
            var codigoPaciente = Guid.NewGuid();
            _pacienteRepoMock.Setup(r => r.Obter(codigoPaciente)).Returns((Paciente)null);

            var request = new AtualizarPacienteDetalhadoRequest
            {
                CodigoPaciente = codigoPaciente,
                DocumentoFederal = "210.576.230-16",
                Nome = "Teste",
                Telefone = "89968484388",
                Email = "teste@email.com"
            };

            // Act
            Action act = () => _service.AtualizarDetalhado(request, Guid.NewGuid());

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Paciente não encontrado");
        }

        [Fact]
        public void Atualizar_ComPacienteNaoEncontrado_DeveLancarExcecao()
        {
            // Arrange
            var codigoPaciente = Guid.NewGuid();
            _pacienteRepoMock.Setup(r => r.Obter(codigoPaciente)).Returns((Paciente)null);

            var request = new AtualizarPacienteRequest
            {
                CodigoPaciente = codigoPaciente,
                DocumentoFederal = "210.576.230-16",
                Nome = "Teste",
                Telefone = "89968484388",
                Email = "teste@email.com"
            };

            // Act
            Action act = () => _service.Atualizar(request, Guid.NewGuid());

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Paciente não encontrado");
        }

        [Fact]
        public void ObterOperacional_ComRelacionamentoValido_DeveRetornarLista()
        {
            // Arrange
            var codigoMedicoUsuarioOperacional = Guid.NewGuid();
            var medicoOperacional = new MedicoUsuarioOperacional
            {
                Codigo = codigoMedicoUsuarioOperacional,
                CodigoUsuario = Guid.NewGuid(),
                CodigoMedico = Guid.NewGuid(),
                Ativo = true
            };

            _medicoUsuarioOperacionalRepository
                .Setup(r => r.Obter(codigoMedicoUsuarioOperacional))
                .Returns(medicoOperacional);

            _pacienteRepoMock
                .Setup(r => r.ObterOperacional(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), 1, 25))
                .Returns(new List<ListarPacientesRawQuery> { new ListarPacientesRawQuery() });

            _pacienteRepoMock
                .Setup(r => r.ObterTotalizadorOperacional(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new CountRawQuery { Count = 1 });

            var request = new ObterOperacionalRequest
            {
                CodigoMedicoUsuarioOperacional = codigoMedicoUsuarioOperacional,
                Pagina = 1,
                ItensPorPagina = 25
            };

            // Act
            var result = _service.ObterOperacional(request);

            // Assert
            result.Should().NotBeNull();
            result.ListaPacientes.Should().HaveCount(1);
        }

        [Fact]
        public void ObterOperacional_ComRelacionamentoNaoEncontrado_DeveLancarExcecao()
        {
            // Arrange
            var codigoMedicoUsuarioOperacional = Guid.NewGuid();
            _medicoUsuarioOperacionalRepository
                .Setup(r => r.Obter(codigoMedicoUsuarioOperacional))
                .Returns((MedicoUsuarioOperacional)null);

            var request = new ObterOperacionalRequest
            {
                CodigoMedicoUsuarioOperacional = codigoMedicoUsuarioOperacional
            };

            // Act
            Action act = () => _service.ObterOperacional(request);

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Relacionamento não encontrado.");
        }

        [Fact]
        public void ObterOperacional_ComRelacionamentoDesativado_DeveLancarExcecao()
        {
            // Arrange
            var codigoMedicoUsuarioOperacional = Guid.NewGuid();
            var medicoOperacional = new MedicoUsuarioOperacional
            {
                Codigo = codigoMedicoUsuarioOperacional,
                Ativo = false
            };

            _medicoUsuarioOperacionalRepository
                .Setup(r => r.Obter(codigoMedicoUsuarioOperacional))
                .Returns(medicoOperacional);

            var request = new ObterOperacionalRequest
            {
                CodigoMedicoUsuarioOperacional = codigoMedicoUsuarioOperacional
            };

            // Act
            Action act = () => _service.ObterOperacional(request);

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Relacionamento desativado.");
        }

        [Fact]
        public void Listar_ComPaginaInvalida_DeveUsarPaginaPadrao()
        {
            // Arrange
            var request = new ListarRequest { Filtro = "teste", Pagina = 0, ItensPorPagina = 10 };
            var codigoParceiro = Guid.NewGuid();

            _pacienteRepoMock.Setup(r => r.Listar(It.IsAny<string>(), codigoParceiro, 1, 10))
                .Returns(new List<ListarPacientesRawQuery>());

            _pacienteRepoMock.Setup(r => r.ObterTotalizador(It.IsAny<string>(), codigoParceiro))
                .Returns(new CountRawQuery { Count = 0 });

            // Act
            var response = _service.Listar(request, codigoParceiro);

            // Assert
            response.Should().NotBeNull();
        }

        [Fact]
        public void Listar_ComItensPorPaginaInvalido_DeveUsarValorPadrao()
        {
            // Arrange
            var request = new ListarRequest { Filtro = "teste", Pagina = 1, ItensPorPagina = 0 };
            var codigoParceiro = Guid.NewGuid();

            _pacienteRepoMock.Setup(r => r.Listar(It.IsAny<string>(), codigoParceiro, 1, 25))
                .Returns(new List<ListarPacientesRawQuery>());

            _pacienteRepoMock.Setup(r => r.ObterTotalizador(It.IsAny<string>(), codigoParceiro))
                .Returns(new CountRawQuery { Count = 0 });

            // Act
            var response = _service.Listar(request, codigoParceiro);

            // Assert
            response.Should().NotBeNull();
        }
    }
}
