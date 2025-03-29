namespace Application.Messaging.Request.Medico
{
    public class ListarMedicoRequest
    {
        public string? Filtro { get; set; }
        public int? Pagina { get; set; } = 1;
        public int? ItensPorPagina { get; set; } = 25;
    }
}
