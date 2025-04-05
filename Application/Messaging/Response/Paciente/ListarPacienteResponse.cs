using Infra.RawQueryResult;

namespace Application.Messaging.Response.Paciente
{
    public class ListarPacienteResponse
    {
        public IEnumerable<ListarPacientesRawQuery> ListaPacientes { get; set; }
        public int TotalPaginas { get; set; }
        public int Itens { get; set; }
    }
}
