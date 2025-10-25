using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Pacientes
{
    public interface IPacienteRepository
    {
        void Inserir(Paciente paciente);
        IEnumerable<ListarPacientesRawQuery> Listar(string filtro, Guid codigoParceiro, int pagina, int itensPorPagina);
        CountRawQuery ObterTotalizador(string filtro, Guid codigoParceiro);
        Paciente Obter(Guid codigo);
        void Atualizar(Paciente paciente);
        Paciente ObterPorDocumentoFederal(string documentoFederal, Guid codigoParceiro);
        List<ListarDocumentosRawQuery> ObterDocumentos(string DocumentoFederal, Guid codigoParceiro);
        IEnumerable<ListarPacientesRawQuery> ObterOperacional(string filtro, Guid codigoUsuario, Guid codigoMedico, int pagina, int itensPorPagina);
        CountRawQuery ObterTotalizadorOperacional(string filtro, Guid codigoUsuario, Guid codigoMedico);
    }
}
