namespace Infra.Helper
{
    public static class DatetimeHelper
    {
        public static DateTime NormalizarInicio(DateTime data)
        {
            return new DateTime(data.Year, data.Month, data.Day, 0, 0, 0);
        }

        public static DateTime NormalizarFim(DateTime data)
        {
            return new DateTime(data.Year, data.Month, data.Day, 23, 59, 59);
        }

        public static DateTime NormalizarInicioSemana(DateTime data)
        {
            var diff = (7 + (data.DayOfWeek - DayOfWeek.Monday)) % 7;
            return data.Date.AddDays(-diff); 
        }

        public static DateTime NormalizarFimSemana(DateTime data)
        {
            var inicioSemana = NormalizarInicioSemana(data);
            return inicioSemana.AddDays(7).AddTicks(-1); 
        }
    }
}
