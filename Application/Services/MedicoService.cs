using Application.Interfaces;
using Application.Messaging.Request.Medico;
using Domain.Entidades;
using Infra.Repositories.Medicos;

namespace Application.Services
{
    public class MedicoService : IMedicoService
    {
        private readonly IMedicoRepository _medicoRepository;

        public MedicoService(IMedicoRepository medicoRepository)
        {
            _medicoRepository = medicoRepository;
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
                CRM = request.CRM
            };

            _medicoRepository.Inserir(medico);
        }
    }
}
