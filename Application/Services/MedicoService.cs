using Application.Interfaces;
using Application.Messaging.Request.Medico;
using Application.Messaging.Response.Medico;
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
                CRM = request.CRM,
                Email = request.Email,
                Telefone = request.Telefone,
                DocumentoFederal = request.DocumentoFederal,
            };

            _medicoRepository.Inserir(medico);
        }

        //public ListarMedicosResponse Listar(ListarMedicoRequest request, Guid codigoUsuario)
        //{
        //    var listaMedicos = _medicoRepository
        //}
    }
}
