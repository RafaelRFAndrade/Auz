using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Usuario;
using Application.Services;
using AutoMapper;
using Domain.Entidades;
using Domain.Enums;
using FluentAssertions;
using Infra.RawQueryResult;
using Infra.Repositories.Agendamentos;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Parceiro;
using Infra.Repositories.Usuarios;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Services
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<IAutenticacaoService> _autenticacaoServiceMock;
        private readonly Mock<IAtendimentoRepository> _atendimentoRepositoryMock;
        private readonly Mock<IAgendamentoRepository> _agendamentoRepositoryMock;
        private readonly Mock<IParceiroRepository> _parceiroRepositoryMock;
        private readonly Mock<IMapper> _mapper;
        private readonly UsuarioService _service;

        public UsuarioServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _autenticacaoServiceMock = new Mock<IAutenticacaoService>();
            _atendimentoRepositoryMock = new Mock<IAtendimentoRepository>();
            _agendamentoRepositoryMock = new Mock<IAgendamentoRepository>();
            _parceiroRepositoryMock = new Mock<IParceiroRepository>(); 
            _mapper = new Mock<IMapper>();

            _service = new UsuarioService(
                _usuarioRepositoryMock.Object,
                _autenticacaoServiceMock.Object,
                _atendimentoRepositoryMock.Object,
                _agendamentoRepositoryMock.Object,
                _parceiroRepositoryMock.Object,
                _mapper.Object
            );
        }

        [Fact]
        public void Cadastrar_DeveInserirUsuarioEParceiro()
        {
            // Arrange
            var request = new CadastroUsuarioRequest
            {
                Email = "teste@teste.com",
                Nome = "Teste",
                Senha = "123456",
                TipoPermissao = TipoPermissao.Admin,
                NomeParceiro = "Parceiro Teste"
            };

            _autenticacaoServiceMock
                .Setup(s => s.Encriptador(It.IsAny<Usuario>(), It.IsAny<string>()))
                .Returns("senhaHash");

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmail(request.Email))
                .Returns((Usuario)null);

            // Act
            _service.Cadastrar(request);

            // Assert
            _parceiroRepositoryMock.Verify(r => r.Inserir(It.IsAny<Parceiro>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.Inserir(It.Is<Usuario>(u => u.Email == request.Email)), Times.Once);
        }

        [Fact]
        public void Cadastrar_DeveLancarExcecao_QuandoEmailJaExiste()
        {
            var request = new CadastroUsuarioRequest
            {
                Email = "teste@teste.com",
                Nome = "Teste",
                Senha = "123456",
                TipoPermissao = TipoPermissao.Admin,
                NomeParceiro = "Parceiro Teste"
            };

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmail(request.Email))
                .Returns(new Usuario());

            // Act
            Action act = () => _service.Cadastrar(request);

            // Assert
            act.Should().Throw<AuzException>().WithMessage("Email já cadastrado.");
        }

        [Fact]
        public void Login_DeveRetornarUsuario_QuandoCredenciaisForemValidas()
        {
            var usuario = new Usuario
            {
                Email = "teste@teste.com",
                Senha = new PasswordHasher<Usuario>().HashPassword(null, "123456")
            };

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmail(usuario.Email))
                .Returns(usuario);

            var request = new LoginRequest { Email = usuario.Email, Senha = "123456" };

            // Act
            var result = _service.Login(request);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(usuario.Email);
        }

        [Fact]
        public void Login_DeveLancarExcecao_QuandoUsuarioNaoExiste()
        {
            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmail(It.IsAny<string>()))
                .Returns((Usuario)null);

            var request = new LoginRequest { Email = "inexistente@teste.com", Senha = "123456" };

            Action act = () => _service.Login(request);

            act.Should().Throw<AuzException>().WithMessage("Usuário não existe.");
        }

        [Fact]
        public void Login_DeveLancarExcecao_QuandoSenhaInvalida()
        {
            var usuario = new Usuario
            {
                Email = "teste@teste.com",
                Senha = new PasswordHasher<Usuario>().HashPassword(null, "correta")
            };

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmail(usuario.Email))
                .Returns(usuario);

            var request = new LoginRequest { Email = usuario.Email, Senha = "errada" };

            Action act = () => _service.Login(request);

            act.Should().Throw<AuzException>().WithMessage("Senha inválida.");
        }

        [Fact]
        public void CarregarRelacionamentos_DeveRetornarValoresHomeResponse()
        {
            var codigoUsuario = Guid.NewGuid();
            var codigoParceiro = Guid.NewGuid();

            _usuarioRepositoryMock.Setup(r => r.ObterNome(codigoUsuario))
                .Returns((new StringRawQuery { String = "Usuario Teste" }));

            // Act
            var result = _service.CarregarRelacionamentos(codigoUsuario, codigoParceiro);

            // Assert
            result.Sucesso.Should().BeTrue();
            result.NomeUsuario.Should().Be("Usuario Teste");
        }
    }
}
