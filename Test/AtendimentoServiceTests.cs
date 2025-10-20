using Application.Messaging.Exception;
using Application.Messaging.Request.Atendimento;
using Application.Messaging.Response.Atendimento;
using Application.Services;
using AutoMapper;
using Domain.Entidades;
using Domain.Enums;
using FluentAssertions;
using Infra.RawQueryResult;
using Infra.Repositories.Agendamentos;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Documentos;
using Infra.Repositories.Medicos;
using Infra.Repositories.Pacientes;
using Moq;

namespace Application.Tests.Services
{
    public class AtendimentoServiceTests
    {
        private readonly Mock<IAtendimentoRepository> _atendimentoRepoMock;
        private readonly Mock<IMedicoRepository> _medicoRepoMock;
        private readonly Mock<IPacienteRepository> _pacienteRepoMock;
        private readonly Mock<IDocumentoRepository> _documentoRepoMock;
        private readonly Mock<IAgendamentoRepository> _agendamentoRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AtendimentoService _service;

        public AtendimentoServiceTests()
        {
            _atendimentoRepoMock = new Mock<IAtendimentoRepository>();
            _medicoRepoMock = new Mock<IMedicoRepository>();
            _pacienteRepoMock = new Mock<IPacienteRepository>();
            _documentoRepoMock = new Mock<IDocumentoRepository>();
            _agendamentoRepoMock = new Mock<IAgendamentoRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new AtendimentoService(
                _atendimentoRepoMock.Object,
                _medicoRepoMock.Object,
                _pacienteRepoMock.Object,
                _documentoRepoMock.Object,
                _agendamentoRepoMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public void Cadastrar_DeveInserirAtendimento_ComPacienteExistente()
        {
            // Arrange
            var medico = new Medico { Codigo = Guid.NewGuid(), DocumentoFederal = "123" };
            var paciente = new Paciente { Codigo = Guid.NewGuid(), DocumentoFederal = "456" };

            _medicoRepoMock.Setup(r => r.ObterPorDocumentoFederal("123", It.IsAny<Guid>()))
                .Returns(medico);

            _pacienteRepoMock.Setup(r => r.ObterPorDocumentoFederal("456", It.IsAny<Guid>()))
                .Returns(paciente);

            var request = new CadastroAtendimentoRequest
            {
                DocumentoFederalMedico = "123",
                DocumentoFederalPaciente = "456",
                CadastrarPaciente = false,
                Descricao = "Consulta de rotina"
            };

            // Act
            _service.Cadastrar(request, Guid.NewGuid(), Guid.NewGuid());

            // Assert
            _atendimentoRepoMock.Verify(r => r.Inserir(It.Is<Atendimento>(
                a => a.CodigoMedico == medico.Codigo &&
                     a.CodigoPaciente == paciente.Codigo &&
                     a.Descricao == "Consulta de rotina"
            )), Times.Once);
        }

        [Fact]
        public void Cadastrar_ComMedicoInexistente_DeveLancarExcecao()
        {
            // Arrange
            _medicoRepoMock.Setup(r => r.ObterPorDocumentoFederal(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns((Medico)null);

            var request = new CadastroAtendimentoRequest
            {
                DocumentoFederalMedico = "123",
                DocumentoFederalPaciente = "456",
                Descricao = "Teste",
            };

            // Act
            Action act = () => _service.Cadastrar(request, Guid.NewGuid(), Guid.NewGuid());

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Médico não encontrado.");
        }

        [Fact]
        public void Cadastrar_ComPacienteInexistenteESemFlagCadastro_DeveLancarExcecao()
        {
            // Arrange
            var medico = new Medico { Codigo = Guid.NewGuid() };
            _medicoRepoMock.Setup(r => r.ObterPorDocumentoFederal(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(medico);

            _pacienteRepoMock.Setup(r => r.ObterPorDocumentoFederal(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns((Paciente)null);

            var request = new CadastroAtendimentoRequest
            {
                DocumentoFederalMedico = "123",
                DocumentoFederalPaciente = "456",
                Descricao = "Teste",
                CadastrarPaciente = false
            };

            // Act
            Action act = () => _service.Cadastrar(request, Guid.NewGuid(), Guid.NewGuid());

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Paciente não encontrado");
        }

        [Fact]
        public void Cadastrar_ComCadastroDeNovoPaciente_DeveInserirPacienteENovoAtendimento()
        {
            // Arrange
            var medico = new Medico { Codigo = Guid.NewGuid() };

            _medicoRepoMock.Setup(r => r.ObterPorDocumentoFederal(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(medico);

            _pacienteRepoMock.Setup(r => r.ObterPorDocumentoFederal(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns((Paciente)null);

            var request = new CadastroAtendimentoRequest
            {
                DocumentoFederalMedico = "123",
                DocumentoFederalPaciente = "456",
                CadastrarPaciente = true,
                NovoPaciente = new()
                {
                    Nome = "João",
                    Email = "joao@email.com",
                    Telefone = "99999999"
                },
                Descricao = "Primeira consulta"
            };

            var codigoUsuario = Guid.NewGuid();

            // Act
            _service.Cadastrar(request, codigoUsuario, Guid.NewGuid());

            // Assert
            _pacienteRepoMock.Verify(r => r.Inserir(It.Is<Paciente>(
                p => p.Nome == "João" && p.Email == "joao@email.com"
            )), Times.Once);

            _atendimentoRepoMock.Verify(r => r.Inserir(It.Is<Atendimento>(
                a => a.Descricao == "Primeira consulta"
            )), Times.Once);
        }

        [Fact]
        public void ListarAtendimentos_DeveRetornarComTotalPaginas()
        {
            // Arrange
            var request = new ListarAtendimentosRequest
            {
                Pagina = 1,
                ItensPorPagina = 10
            };

            _atendimentoRepoMock.Setup(r => r.ListarAtendimentos(It.IsAny<Guid>(), 1, 10, It.IsAny<string>()))
                .Returns(new List<ListarAtendimentosRawQuery> { new ListarAtendimentosRawQuery() });

            _atendimentoRepoMock.Setup(r => r.TotalizarAtendimentos(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new CountRawQuery { Count = 10 });

            // Act
            var result = _service.ListarAtendimentos(request, Guid.NewGuid(), Guid.NewGuid());

            // Assert
            result.Atendimentos.Should().HaveCount(1);
            result.Itens.Should().Be(10);
            result.TotalPaginas.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Deletar_ComAtendimentoExistente_DeveAtualizarSituacaoParaDesativo()
        {
            // Arrange
            var atendimento = new Atendimento
            {
                Codigo = Guid.NewGuid(),
                Situacao = Situacao.Ativo
            };

            _atendimentoRepoMock.Setup(r => r.Obter(atendimento.Codigo))
                .Returns(atendimento);

            // Act
            _service.Deletar(atendimento.Codigo);

            // Assert
            _atendimentoRepoMock.Verify(r => r.Atualizar(It.Is<Atendimento>(
                a => a.Situacao == Situacao.Desativo
            )), Times.Once);
        }

        [Fact]
        public void Deletar_ComAtendimentoInexistente_DeveLancarExcecao()
        {
            // Arrange
            _atendimentoRepoMock.Setup(r => r.Obter(It.IsAny<Guid>()))
                .Returns((Atendimento)null);

            // Act
            Action act = () => _service.Deletar(Guid.NewGuid());

            // Assert
            act.Should().Throw<AuzException>()
                .WithMessage("Atendimento não encontrado.");
        }

        [Fact]
        public void ObterRelacionamentos_DeveRetornarResponseComDocumentosEAgendamentos()
        {
            // Arrange
            var codigo = Guid.NewGuid();
            var atendimento = new ObterAtendimentoRawQuery { CodigoAtendimento = codigo };

            var documentos = new List<DocumentoRawQuery> { new DocumentoRawQuery() };
            var agendamentos = new List<ObterAgendamentosPorAtendimentoRawQuery> { new ObterAgendamentosPorAtendimentoRawQuery() };

            _atendimentoRepoMock.Setup(r => r.ObterAtendimento(codigo)).Returns(atendimento);
            _documentoRepoMock.Setup(r => r.ObterDocumentosPorCodigoEntidade(codigo)).Returns(documentos);
            _agendamentoRepoMock.Setup(r => r.ObterAgendamentosPorAtendimento(codigo)).Returns(agendamentos);

            _mapperMock
                .Setup(m => m.Map(It.IsAny<ObterAtendimentoRawQuery>(), It.IsAny<ObterAtendimentoResponse>()))
                .Callback<ObterAtendimentoRawQuery, ObterAtendimentoResponse>((src, dest) =>
                {
                    dest.DescricaoAtendimento = "Consulta Teste";
                });

            // Act
            var response = _service.ObterRelacionamentos(codigo);

            // Assert
            response.DescricaoAtendimento.Should().Be("Consulta Teste");
            response.Documentos.Should().HaveCount(1);
            response.Agendamentos.Should().HaveCount(1);
        }
    }
}