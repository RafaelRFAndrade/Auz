namespace Application.Messaging.Request.Parceiro
{
    public class AtualizarParceiroRequest
    {
        public Guid Codigo { get; set; }
        public string Nome { get; set; } = null!;

        public bool Admin { get; set; }

        public string? RazaoSocial { get; set; }

        public string? Cnpj { get; set; }

        public string? Cep { get; set; }

        public string? Logradouro { get; set; }

        public string? Numero { get; set; }

        public string? Complemento { get; set; }

        public string? Bairro { get; set; }

        public string? Cidade { get; set; }

        public string? Uf { get; set; }

        public string? Telefone { get; set; }

        public string? Email { get; set; }
    }
}
