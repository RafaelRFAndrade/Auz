using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Atendimentos
{
    public interface IAtendimentoRepository
    {
        void Inserir(Atendimento atendimento);
        IEnumerable<ObterAtendimentosRawQuery> ObterAtendimentosPorCodigoParceiro(Guid codigoParceiro);
        bool ValidarAtendimentoAtivosPorMedico(Guid codigoMedico);
        bool ValidarAtendimentoAtivosPorPaciente(Guid codigoPaciente);
        List<ListarAtendimentosRawQuery> ListarAtendimentos(Guid codigoParceiro, int pagina, int itensPorPagina, string filtro);
        CountRawQuery TotalizarAtendimentos(Guid codigoParceiro, string filtro);
        void Deletar(Atendimento atendimento);
        Atendimento Obter(Guid codigoAtendimento);
        void Atualizar(Atendimento atendimento);
        ObterAtendimentoRawQuery ObterAtendimento(Guid codigoAtendimento);
        List<ListarAtendimentosPorMedicoRawQuery> ListarAtendimentosPorMedico(Guid codigoMedico);
    }
}
