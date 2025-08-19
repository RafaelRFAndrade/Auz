using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Medicos
{
    public interface IMedicoRepository
    {
        void Inserir(Medico medico);
        IEnumerable<ListarMedicoRawQuery> Listar(string filtro, Guid codigoUsuario, int pagina, int itensPorPagina);
        CountRawQuery ObterTotalizador(string filtro, Guid codigoUsuario);
        Medico Obter(Guid codigo);
        void Atualizar(Medico medico);
        Medico ObterPorDocumentoFederal(string DocumentoFederal, Guid codigoParceiro);
    }
}
