using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Usuario;
using Application.Messaging.Response.Usuario;
using Domain.Entidades;
using Domain.Enums;
using Infra.Repositories.Agendamentos;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Usuarios;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAutenticacaoService _autenticacaoService;
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IAtendimentoRepository _atendimentoRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository,
            IAutenticacaoService autenticacaoService,
            IAtendimentoRepository atendimentoRepository,
            IAgendamentoRepository agendamentoRepository) 
        {
            _usuarioRepository = usuarioRepository;
            _autenticacaoService = autenticacaoService;
            _agendamentoRepository = agendamentoRepository;
            _atendimentoRepository = atendimentoRepository;
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

            _usuarioRepository.Inserir(usuario);
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

        public ValoresHomeResponse CarregarRelacionamentos(Guid codigoUsuario)
        {
            var nomeUsuario = _usuarioRepository.ObterNome(codigoUsuario);
            var atendimentos = _atendimentoRepository.ObterAtendimentosPorCodigoUsuario(codigoUsuario);

            var agendamentos = _agendamentoRepository.ObterAgendamentosPorCodigoUsuario(codigoUsuario);

            return new ValoresHomeResponse
            {
                NomeUsuario = nomeUsuario.String,
                Agendamentos = agendamentos,
                Atendimentos = atendimentos,
                Sucesso = true
            };
        }
    }
}
