using Infra.RawQueryResult;

namespace Application.Messaging.Response.Medico
{
    public class ListarMedicosResponse
    {
       public IEnumerable<ListarMedicoRawQuery> ListaMedicos { get; set; }
       public int TotalPaginas {  get; set; }
       public int Itens {  get; set; }
    }
}
