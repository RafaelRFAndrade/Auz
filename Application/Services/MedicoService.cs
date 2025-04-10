﻿using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request;
using Application.Messaging.Request.Medico;
using Application.Messaging.Response.Medico;
using AutoMapper;
using Domain.Entidades;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Medicos;

namespace Application.Services
{
    public class MedicoService : IMedicoService
    {
        private readonly IMedicoRepository _medicoRepository;
        private readonly IMapper _mapper;
        private readonly IAtendimentoRepository _atendimentoRepository;

        public MedicoService(IMedicoRepository medicoRepository, IMapper mapper, IAtendimentoRepository atendimentoRepository)
        {
            _medicoRepository = medicoRepository;
            _mapper = mapper;
            _atendimentoRepository = atendimentoRepository;
        }

        public void Cadastrar(CadastroMedicoRequest request, Guid codigoUsuario)
        {
            request.Validar();

            var medico = new Medico
            {
                Codigo = Guid.NewGuid(),
                CodigoUsuario = codigoUsuario,
                Nome = request.Nome,
                Situacao = Domain.Enums.Situacao.Ativo,
                DtInclusao = DateTime.Now,
                CRM = request.CRM,
                Email = request.Email,
                Telefone = request.Telefone,
                DocumentoFederal = request.DocumentoFederal,
            };

            _medicoRepository.Inserir(medico);
        }

        public ListarMedicosResponse Listar(ListarRequest request, Guid codigoUsuario)
        {
            var listaMedicos = _medicoRepository.Listar(request.Filtro, codigoUsuario, request.Pagina.GetValueOrDefault(), request.ItensPorPagina.GetValueOrDefault());

            var totalizador = _medicoRepository.ObterTotalizador(request.Filtro, codigoUsuario);

            var total = totalizador.Count / request.ItensPorPagina.GetValueOrDefault(25);

            return new ListarMedicosResponse
            {
                ListaMedicos = listaMedicos,
                TotalPaginas = total == 0 ? 25 : total,
                Itens = totalizador.Count
            };
        }

        public Medico Obter(Guid codigoMedico, Guid codigoUsuario)
        {
            var medico = _medicoRepository.Obter(codigoMedico);

            if (medico.CodigoUsuario != codigoUsuario)
                throw new AuzException("Usuário não tem permissão para vizualizar esse médico");

            return medico;
        }

        public void Atualizar(AtualizarMedicoRequest request, Guid codigoUsuario)
        {
            request.Validar();

            var medico = _medicoRepository.Obter(request.Codigo) ??
                throw new AuzException("Médico não encontrado");

            if (medico.CodigoUsuario != codigoUsuario)
                throw new AuzException("Usuário não possuí permissão para alterar o médico");

            if (medico.DocumentoFederal != request.DocumentoFederal)
                throw new AuzException("Não é possível alterar o documento federal.");

            _mapper.Map(request, medico);

            medico.DtSituacao = DateTime.Now;

            _medicoRepository.Atualizar(medico);
        }

        public void Desativar(DesativarMedicoRequest request, Guid codigoUsuario)
        {
            var medico = _medicoRepository.Obter(request.CodigoMedico) ??
                throw new AuzException("Médico não encontrado");

            if (medico.CodigoUsuario != codigoUsuario)
                throw new AuzException("Usuário não possuí permissão para alterar o médico");

            if(_atendimentoRepository.ValidarAtendimentoAtivosPorMedico(medico.Codigo))
                throw new AuzException("Médico possuí atendimentos em andamento");

            medico.Situacao = Domain.Enums.Situacao.Desativo;
            medico.DtSituacao = DateTime.Now;

            _medicoRepository.Atualizar(medico);
        }
    }
}
