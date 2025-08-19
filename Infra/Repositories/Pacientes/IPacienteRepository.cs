using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Pacientes
{
    public interface IPacienteRepository
    {
        void Inserir(Paciente paciente);
        IEnumerable<ListarPacientesRawQuery> Listar(string filtro, Guid codigoUsuario, int pagina, int itensPorPagina);
        CountRawQuery ObterTotalizador(string filtro, Guid codigoUsuario);
        Paciente Obter(Guid codigo);
        void Atualizar(Paciente paciente);
        Paciente ObterPorDocumentoFederal(string documentoFederal, Guid codigoParceiro);
    }
}
