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
    }
}
