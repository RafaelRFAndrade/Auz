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
            var codigoMedico = Guid.NewGuid();
            var medico = new Medico
            {
                Codigo = codigoMedico,
                CodigoUsuario = codigoUsuario,
                DocumentoFederal = "689.683.340-80"
            };

            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns(medico);

            var request = new AtualizarMedicoRequest
            {
                Codigo = codigoMedico,
                DocumentoFederal = "226.362.150-42", // diferente do cadastrado
                Nome = "Teste",
                CRM = "20000SC",
                Email = "teste@gmail.com",
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

        [Fact]
        public void Obter_ComMedicoExistente_DeveRetornarMedico()
        {
            // Arrange
            var codigoMedico = Guid.NewGuid();
            var medico = new Medico 
            { 
                Codigo = codigoMedico,
                Nome = "Dr. Teste",
                CRM = "12345"
            };
            
            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns(medico);

            // Act
            var result = _service.Obter(codigoMedico, Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Codigo.Should().Be(codigoMedico);
            result.Nome.Should().Be("Dr. Teste");
        }

        [Fact]
        public void Obter_ComMedicoNaoEncontrado_DeveRetornarNull()
        {
            // Arrange
            var codigoMedico = Guid.NewGuid();
            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns((Medico)null);

            // Act
            var result = _service.Obter(codigoMedico, Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ObterMedicoDetalhado_ComMedicoExistente_DeveRetornarDetalhes()
        {
            // Arrange
            var codigoMedico = Guid.NewGuid();
            var medico = new Medico 
            { 
                Codigo = codigoMedico,
                Nome = "Dr. Teste"
            };
            
            var atendimentos = new List<ListarAtendimentosPorMedicoRawQuery> 
            { 
                new ListarAtendimentosPorMedicoRawQuery()
            };

            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns(medico);
            _atendimentoRepoMock.Setup(r => r.ListarAtendimentosPorMedico(codigoMedico)).Returns(atendimentos);

            // Act
            var result = _service.ObterMedicoDetalhado(codigoMedico);

            // Assert
            result.Should().NotBeNull();
            result.Medico.Should().Be(medico);
            result.Atendimentos.Should().HaveCount(1);
        }

        [Fact]
        public void ObterMedicoDetalhado_ComMedicoNaoEncontrado_DeveLancarExcecao()
        {
            // Arrange
            var codigoMedico = Guid.NewGuid();
            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns((Medico)null);

            // Act
            Action act = () => _service.ObterMedicoDetalhado(codigoMedico);

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Médico não encontrado.");
        }

        [Fact]
        public void Atualizar_ComDadosValidos_DeveAtualizarMedico()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var codigoMedico = Guid.NewGuid();
            var medico = new Medico
            {
                Codigo = codigoMedico,
                CodigoUsuario = codigoUsuario,
                DocumentoFederal = "689.683.340-80",
                Nome = "Nome Antigo"
            };

            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns(medico);

            var request = new AtualizarMedicoRequest
            {
                Codigo = codigoMedico,
                DocumentoFederal = "689.683.340-80",
                Nome = "Nome Novo",
                CRM = "12345",
                Email = "novo@email.com",
                Telefone = "89968484388"
            };

            // Act
            _service.Atualizar(request, codigoUsuario);

            // Assert
            _medicoRepoMock.Verify(r => r.Atualizar(It.IsAny<Medico>()), Times.Once);
        }

        [Fact]
        public void Atualizar_ComMedicoNaoEncontrado_DeveLancarExcecao()
        {
            // Arrange
            var codigoMedico = Guid.NewGuid();
            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns((Medico)null);

            var request = new AtualizarMedicoRequest
            {
                Codigo = codigoMedico,
                DocumentoFederal = "689.683.340-80",
                Nome = "Teste",
                CRM = "12345",
                Email = "teste@email.com",
                Telefone = "89968484388"
            };

            // Act
            Action act = () => _service.Atualizar(request, Guid.NewGuid());

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Médico não encontrado");
        }

        [Fact]
        public void Desativar_ComMedicoValido_DeveDesativarMedico()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var codigoMedico = Guid.NewGuid();
            var medico = new Medico
            {
                Codigo = codigoMedico,
                CodigoUsuario = codigoUsuario
            };

            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns(medico);
            _atendimentoRepoMock.Setup(r => r.ValidarAtendimentoAtivosPorMedico(codigoMedico)).Returns(false);

            var request = new DesativarMedicoRequest { CodigoMedico = codigoMedico };

            // Act
            _service.Desativar(request, codigoUsuario);

            // Assert
            medico.Situacao.Should().Be(Domain.Enums.Situacao.Desativo);
            _medicoRepoMock.Verify(r => r.Atualizar(medico), Times.Once);
        }

        [Fact]
        public void Listar_ComListaVazia_DeveRetornarListaVazia()
        {
            // Arrange
            var request = new ListarRequest { Filtro = "teste", Pagina = 1, ItensPorPagina = 10 };
            var codigoUsuario = Guid.NewGuid();

            _medicoRepoMock.Setup(r => r.Listar(It.IsAny<string>(), codigoUsuario, 1, 10))
                .Returns(new List<ListarMedicoRawQuery>());

            _medicoRepoMock.Setup(r => r.ObterTotalizador(It.IsAny<string>(), codigoUsuario))
                .Returns(new CountRawQuery { Count = 0 });

            // Act
            var response = _service.Listar(request, codigoUsuario);

            // Assert
            response.ListaMedicos.Should().BeEmpty();
            response.Itens.Should().Be(0);
            response.TotalPaginas.Should().Be(1);
        }

        [Fact]
        public void ObterPorDocumento_ComDocumentoInexistente_DeveRetornarNull()
        {
            // Arrange
            _medicoRepoMock.Setup(r => r.ObterPorDocumentoFederal(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns((Medico)null);

            // Act
            var result = _service.ObterPorDocumento("999.999.999-99", Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void BuscarDocumentos_ComListaVazia_DeveRetornarListaVazia()
        {
            // Arrange
            _medicoRepoMock.Setup(r => r.ObterDocumentos(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(new List<ListarDocumentosRawQuery>());

            // Act
            var result = _service.BuscarDocumentos("123", Guid.NewGuid());

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void AtualizarCompleto_ComDadosValidos_DeveAtualizarMedico()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var codigoMedico = Guid.NewGuid();
            var medico = new Medico
            {
                Codigo = codigoMedico,
                CodigoUsuario = codigoUsuario,
                DocumentoFederal = "689.683.340-80"
            };

            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns(medico);

            var request = new AtualizarCompletoRequest
            {
                Codigo = codigoMedico,
                DocumentoFederal = "689.683.340-80",
                Nome = "Nome Atualizado",
                CRM = "12345",
                Email = "teste@email.com",
                Telefone = "89968484388"
            };

            // Act
            _service.AtualizarCompleto(request, codigoUsuario);

            // Assert
            _medicoRepoMock.Verify(r => r.Atualizar(It.IsAny<Medico>()), Times.Once);
        }

        [Fact]
        public void AtualizarCompleto_ComMedicoNaoEncontrado_DeveLancarExcecao()
        {
            // Arrange
            var codigoMedico = Guid.NewGuid();
            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns((Medico)null);

            var request = new AtualizarCompletoRequest
            {
                Codigo = codigoMedico,
                DocumentoFederal = "689.683.340-80",
                Nome = "Teste",
                CRM = "12345",
                Email = "teste@email.com",
                Telefone = "89968484388"
            };

            // Act
            Action act = () => _service.AtualizarCompleto(request, Guid.NewGuid());

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Médico não encontrado");
        }

        [Fact]
        public void AtualizarCompleto_ComDocumentoFederalDiferente_DeveLancarExcecao()
        {
            // Arrange
            var codigoUsuario = Guid.NewGuid();
            var codigoMedico = Guid.NewGuid();
            var medico = new Medico
            {
                Codigo = codigoMedico,
                DocumentoFederal = "689.683.340-80"
            };

            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns(medico);

            var request = new AtualizarCompletoRequest
            {
                Codigo = codigoMedico,
                DocumentoFederal = "226.362.150-42",
                Nome = "Teste",
                CRM = "12345",
                Email = "teste@email.com",
                Telefone = "89968484388"
            };

            // Act
            Action act = () => _service.AtualizarCompleto(request, codigoUsuario);

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Não é possível alterar o documento federal.");
        }

        [Fact]
        public void Desativar_ComMedicoNaoEncontrado_DeveLancarExcecao()
        {
            // Arrange
            var codigoMedico = Guid.NewGuid();
            _medicoRepoMock.Setup(r => r.Obter(codigoMedico)).Returns((Medico)null);

            var request = new DesativarMedicoRequest { CodigoMedico = codigoMedico };

            // Act
            Action act = () => _service.Desativar(request, Guid.NewGuid());

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Médico não encontrado");
        }

        [Fact]
        public void ObterUsuariosVinculados_DeveRetornarListaComTotalPaginas()
        {
            // Arrange
            var codigoMedico = Guid.NewGuid();
            var request = new UsuariosVinculadosRequest
            {
                CodigoMedico = codigoMedico,
                Pagina = 1,
                ItensPorPagina = 10
            };

            _medicoUsuarioOperacionalRepoMock
                .Setup(r => r.ListarUsuariosVinculados(codigoMedico, 1, 10))
                .Returns(new List<UsuariosVinculadosRawQuery> { new UsuariosVinculadosRawQuery() });

            _medicoUsuarioOperacionalRepoMock
                .Setup(r => r.TotalizarUsuariosVinculados(codigoMedico))
                .Returns(new CountRawQuery { Count = 5 });

            // Act
            var response = _service.ObterUsuariosVinculados(request);

            // Assert
            response.UsuariosVinculados.Should().HaveCount(1);
            response.Itens.Should().Be(5);
            response.TotalPaginas.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ObterUsuariosVinculados_ComListaVazia_DeveRetornarListaVazia()
        {
            // Arrange
            var codigoMedico = Guid.NewGuid();
            var request = new UsuariosVinculadosRequest
            {
                CodigoMedico = codigoMedico,
                Pagina = 1,
                ItensPorPagina = 10
            };

            _medicoUsuarioOperacionalRepoMock
                .Setup(r => r.ListarUsuariosVinculados(codigoMedico, 1, 10))
                .Returns(new List<UsuariosVinculadosRawQuery>());

            _medicoUsuarioOperacionalRepoMock
                .Setup(r => r.TotalizarUsuariosVinculados(codigoMedico))
                .Returns(new CountRawQuery { Count = 0 });

            // Act
            var response = _service.ObterUsuariosVinculados(request);

            // Assert
            response.UsuariosVinculados.Should().BeEmpty();
            response.Itens.Should().Be(0);
            response.TotalPaginas.Should().Be(1);
        }

        [Fact]
        public void Listar_ComMultiplosItens_DeveRetornarListaCompleta()
        {
            // Arrange
            var request = new ListarRequest { Filtro = "teste", Pagina = 1, ItensPorPagina = 10 };
            var codigoUsuario = Guid.NewGuid();

            _medicoRepoMock.Setup(r => r.Listar(It.IsAny<string>(), codigoUsuario, 1, 10))
                .Returns(new List<ListarMedicoRawQuery> 
                { 
                    new ListarMedicoRawQuery { Nome = "Medico1" },
                    new ListarMedicoRawQuery { Nome = "Medico2" },
                    new ListarMedicoRawQuery { Nome = "Medico3" }
                });

            _medicoRepoMock.Setup(r => r.ObterTotalizador(It.IsAny<string>(), codigoUsuario))
                .Returns(new CountRawQuery { Count = 3 });

            // Act
            var response = _service.Listar(request, codigoUsuario);

            // Assert
            response.ListaMedicos.Should().HaveCount(3);
            response.Itens.Should().Be(3);
        }
    }
}
