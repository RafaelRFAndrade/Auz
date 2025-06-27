using Application.Interfaces;
using Application.Messaging.Request.Atendimento;
using Application.Messaging.Response.Atendimento;
using Application.Messaging.Response.Paciente;
using Domain.Entidades;
using Infra.RawQueryResult;
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

        public ListarAtendimentosResponse ListarAtendimentos(ListarAtendimentosRequest request, Guid codigoUsuario)
        {
            var atendimentos = _atendimentoRepository.ListarAtendimentos(codigoUsuario, request.Pagina.GetValueOrDefault(), request.ItensPorPagina.GetValueOrDefault());

            var totalizador = _atendimentoRepository.TotalizarAtendimentos(codigoUsuario);

            var total = totalizador.Count / request.ItensPorPagina.GetValueOrDefault(25);

            return new ListarAtendimentosResponse
            {
                Atendimentos = atendimentos,
                TotalPaginas = total == 0 ? 25 : total,
                Itens = totalizador.Count
            };
        }
    }
}
