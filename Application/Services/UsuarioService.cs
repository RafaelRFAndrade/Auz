using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Usuario;
using Application.Messaging.Response.Usuario;
using AutoMapper;
using Domain.Entidades;
using Domain.Enums;
using Infra.Repositories.Agendamentos;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.MedicoUsuarioOperacional;
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
        private readonly IMapper _mapper;
        private readonly IMedicoUsuarioOperacionalRepository _medicoUsuarioOperacionalRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository,
            IAutenticacaoService autenticacaoService,
            IAtendimentoRepository atendimentoRepository,
            IAgendamentoRepository agendamentoRepository,
            IParceiroRepository parceiroRepository,
            IMapper mapper,
            IMedicoUsuarioOperacionalRepository medicoUsuarioOperacionalRepository)
        {
            _usuarioRepository = usuarioRepository;
            _autenticacaoService = autenticacaoService;
            _agendamentoRepository = agendamentoRepository;
            _atendimentoRepository = atendimentoRepository;
            _parceiroRepository = parceiroRepository;
            _mapper = mapper;
            _medicoUsuarioOperacionalRepository = medicoUsuarioOperacionalRepository;
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
                Nome = request.NomeParceiro,
                CodigoUsuarioResponsavel = usuario.Codigo,
                CodigoUsuarioInclusao = usuario.Codigo
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
            var pagina = Math.Max(1, request.Pagina.GetValueOrDefault(1));

            var itensPorPagina = request.ItensPorPagina.GetValueOrDefault(25);
            if (itensPorPagina <= 0) itensPorPagina = 25;

            var usuarios = _usuarioRepository.ObterUsuariosPorParceiro(codigoParceiro, pagina, itensPorPagina, request.Filtro);

            var totalizador = _usuarioRepository.ObterTotalizador(codigoParceiro, request.Filtro);

            var totalItens = totalizador?.Count ?? 0;

            var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
            totalPaginas = Math.Max(1, totalPaginas);

            return new ObterUsuariosParceiroResponse
            {
                Usuarios = usuarios,
                TotalPaginas = totalPaginas,
                Itens = totalItens
            };
        }

        public void CadastrarParceiroJaExistente(CadastroUsuarioParceiroJaExistenteRequest request, Guid codigoParceiro)
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
                Senha = request.Senha,
                CodigoParceiro = codigoParceiro
            };

            usuario.Senha = _autenticacaoService.Encriptador(usuario, usuario.Senha);

            var usuarioJaExistente = _usuarioRepository.ObterPorEmail(request.Email);

            if (usuarioJaExistente is not null)
                throw new AuzException("Email já cadastrado.");

            _usuarioRepository.Inserir(usuario);
        }

        public void Desativar(DesativarUsuarioRequest request)
        {
            var usuario = _usuarioRepository.Obter(request.CodigoUsuario) ??
                    throw new AuzException("Usuário não encontrado.");

            usuario.Situacao = Situacao.Desativo;
            usuario.DtSituacao = DateTime.Now;

            _usuarioRepository.Atualizar(usuario);
        }

        public void Atualizar(AtualizarUsuarioRequest request)
        {
            request.Validar();

            var usuario = _usuarioRepository.Obter(request.Codigo) ??
                    throw new AuzException("Usuário não encontrado.");

            _mapper.Map(request, usuario);

            _usuarioRepository.Atualizar(usuario);
        }

        public void RelacionarUsuarioMedico(RelacionarMedicoUsuarioRequest request, TipoPermissao tipoPermissao, Guid codigoUsuario)
        {
            if (request.CodigoMedico == Guid.Empty)
                throw new AuzException("Código não informado.");

            var medicoUsuarioOperacional = new MedicoUsuarioOperacional
            {
                Codigo = Guid.NewGuid(),
                CodigoMedico = request.CodigoMedico,
                CodigoUsuario = codigoUsuario,
                Permissao = ObterVisualizacao(tipoPermissao),
                DtInclusao = DateTime.Now,
                Ativo = true
            };

            _medicoUsuarioOperacionalRepository.Inserir(medicoUsuarioOperacional);
        }

        public ObterMedicoUsuarioResponse ObterRelacionamentos(ListarRequest listarRequest, Guid codigoUsuario)
        {
            var pagina = Math.Max(1, listarRequest.Pagina.GetValueOrDefault(1));

            var itensPorPagina = listarRequest.ItensPorPagina.GetValueOrDefault(25);
            if (itensPorPagina <= 0) itensPorPagina = 25;

            var relacionamentos = _medicoUsuarioOperacionalRepository.Listar(codigoUsuario, pagina, itensPorPagina, listarRequest.Filtro);

            var totalizador = _medicoUsuarioOperacionalRepository.TotalizarRelacionamentos(codigoUsuario, listarRequest.Filtro);

            var totalItens = totalizador?.Count ?? 0;

            var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
            totalPaginas = Math.Max(1, totalPaginas);

            return new ObterMedicoUsuarioResponse
            {
                MedicoUsuario = relacionamentos,
                TotalPaginas = totalPaginas,
                Itens = totalItens
            };
        }

        private TipoVisualizacao ObterVisualizacao(TipoPermissao tipoPermissao)
        {
            return tipoPermissao switch
            {
                TipoPermissao.Operador => TipoVisualizacao.Total,
                TipoPermissao.Admin => TipoVisualizacao.Total,
                TipoPermissao.Standard => TipoVisualizacao.Visualização,
                _ => TipoVisualizacao.Visualização,
            };
        }
    }
}
