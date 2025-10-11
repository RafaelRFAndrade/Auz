namespace Domain.Enums
{
    [Flags]
    public enum DiasAtendimento
    {
        Nenhum = 0,
        Segunda = 1 << 0,  // 1
        Terca = 1 << 1,    // 2
        Quarta = 1 << 2,   // 4
        Quinta = 1 << 3,   // 8
        Sexta = 1 << 4,    // 16
        Sabado = 1 << 5,   // 32
        Domingo = 1 << 6   // 64
    }
}
