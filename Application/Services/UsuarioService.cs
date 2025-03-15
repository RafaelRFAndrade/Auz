using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Domain.Entidades;
using Domain.Enums;
using Infra.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAutenticacaoService _autenticacaoService;

        public UsuarioService(IUsuarioRepository usuarioRepository, IAutenticacaoService autenticacaoService) 
        {
            _usuarioRepository = usuarioRepository;
            _autenticacaoService = autenticacaoService;
        }

        public void Cadastrar(CadastroUsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nome)
                || string.IsNullOrWhiteSpace(request.Senha)
                || string.IsNullOrWhiteSpace(request.Email))
                throw new AuzException("Campos ausentes");

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

            _usuarioRepository.Inserir(usuario);
        }

        public Usuario Login(Messaging.Request.LoginRequest request)
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
    }
}
