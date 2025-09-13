using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Usuario;
using Application.Messaging.Response.Usuario;
using Azure.Core;
using Domain.Entidades;
using Domain.Enums;
using Infra.Repositories.Agendamentos;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Parceiro;
using Infra.Repositories.Usuarios;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAutenticacaoService _autenticacaoService;
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IAtendimentoRepository _atendimentoRepository;
        private readonly IParceiroRepository _parceiroRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository,
            IAutenticacaoService autenticacaoService,
            IAtendimentoRepository atendimentoRepository,
            IAgendamentoRepository agendamentoRepository,
            IParceiroRepository parceiroRepository)
        {
            _usuarioRepository = usuarioRepository;
            _autenticacaoService = autenticacaoService;
            _agendamentoRepository = agendamentoRepository;
            _atendimentoRepository = atendimentoRepository;
            _parceiroRepository = parceiroRepository;
        }

        public void Cadastrar(CadastroUsuarioRequest request)
        {
            request.Validar();

            var usuario = new Usuario
            {
                Codigo = Guid.NewGuid(),
                Situacao = Situacao.Ativo,
                Email = request.Email,
                Nome = request.Nome,
                DtInclusao = DateTime.Now,
                TipoPermissao = request.TipoPermissao,
                Senha = request.Senha
            };

            usuario.Senha = _autenticacaoService.Encriptador(usuario, usuario.Senha);

            var usuarioJaExistente = _usuarioRepository.ObterPorEmail(request.Email);

            if (usuarioJaExistente is not null)
                throw new AuzException("Email já cadastrado.");

            var parceiro = new Parceiro
            {
                Codigo = Guid.NewGuid(),
                Situacao = Situacao.Ativo,
                DtInclusao = DateTime.Now,
                DtSituacao = DateTime.Now,
                Admin = false,
                Nome = request.NomeParceiro
            };

            usuario.CodigoParceiro = parceiro.Codigo;

            using (var transactionScope = new TransactionScope())
            {
                _parceiroRepository.Inserir(parceiro);
                _usuarioRepository.Inserir(usuario);

                transactionScope.Complete();
            }
        }
    
      public Usuario Login(LoginRequest request)
        {
            var usuario = _usuarioRepository.ObterPorEmail(request.Email);

            if (usuario is null)
                throw new AuzException("Usuário não existe.");

            var passwordHasher = new PasswordHasher<Usuario>();

            var verificarSenha = passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, request.Senha);

            if (verificarSenha == PasswordVerificationResult.Failed)
                throw new AuzException("Senha inválida.");

            return usuario;
        }

        public ValoresHomeResponse CarregarRelacionamentos(Guid codigoUsuario, Guid codigoParceiro)
        {
            var nomeUsuario = _usuarioRepository.ObterNome(codigoUsuario);
            var atendimentos = _atendimentoRepository.ObterAtendimentosPorCodigoUsuario(codigoUsuario);

            var agendamentos = _agendamentoRepository.ObterAgendamentosPorCodigoUsuario(codigoParceiro);

            return new ValoresHomeResponse
            {
                NomeUsuario = nomeUsuario.String,
                Agendamentos = agendamentos,
                Atendimentos = atendimentos,
                Sucesso = true
            };
        }

        public ObterUsuariosParceiroResponse ObterUsuariosPorParceiro(Guid codigoParceiro, ObterUsuariosPorParceiroRequest request)
        {
            var usuarios = _usuarioRepository.ObterUsuariosPorParceiro(codigoParceiro, request.Pagina.GetValueOrDefault(1), request.Itens.GetValueOrDefault(25));

            var totalizador = _usuarioRepository.ObterTotalizador(codigoParceiro);

            var total = totalizador.Count / request.Itens.GetValueOrDefault(25);

            return new ObterUsuariosParceiroResponse
            {
                Usuarios = usuarios,
                TotalPaginas = total == 0 ? 25 : total,
                Itens = totalizador.Count
            };
        }
    }
}
