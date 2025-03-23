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
    }
}
