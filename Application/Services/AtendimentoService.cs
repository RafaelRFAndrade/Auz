using Application.Interfaces;
using Application.Messaging.Request.Atendimento;
using Domain.Entidades;
using Infra.Repositories.Atendimentos;

namespace Application.Services
{
    public class AtendimentoService : IAtendimentoService
    {
        private readonly IAtendimentoRepository _atendimentoRepository;

        public AtendimentoService(IAtendimentoRepository atendimentoRepository)
        {
            _atendimentoRepository = atendimentoRepository;
        }

        public void Cadastrar(CadastroAtendimentoRequest request, Guid codigoUsuario)
        {
            request.Validar();

            var atendimento = new Atendimento
            {
                Codigo = Guid.NewGuid(),
                CodigoMedico = request.CodigoMedico,
                CodigoPaciente = request.CodigoPaciente,
                CodigoUsuario = codigoUsuario,
                Situacao = Domain.Enums.Situacao.Ativo,
                DtInclusao = DateTime.Now,
                Descricao = request.Descricao
            };

            _atendimentoRepository.Inserir(atendimento); 
        }
    }
}
