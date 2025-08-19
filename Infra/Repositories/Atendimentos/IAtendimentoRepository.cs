using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Atendimentos
{
    public interface IAtendimentoRepository
    {
        void Inserir(Atendimento atendimento);
        IEnumerable<ObterAtendimentosRawQuery> ObterAtendimentosPorCodigoUsuario(Guid codigoUsuario);
        bool ValidarAtendimentoAtivosPorMedico(Guid codigoMedico);
        bool ValidarAtendimentoAtivosPorPaciente(Guid codigoPaciente);
        List<ListarAtendimentosRawQuery> ListarAtendimentos(Guid codigoParceiro, int pagina, int itensPorPagina);
        CountRawQuery TotalizarAtendimentos(Guid codigoUsuario);
    }
}
