namespace Application.Messaging.Request
{
    public class ListarRequest
    {
        public string? Filtro { get; set; }
        public int? Pagina { get; set; } = 1;
        public int? ItensPorPagina { get; set; } = 25;
    }
}
