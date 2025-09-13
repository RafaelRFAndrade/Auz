using Domain.Enums;

namespace Infra.RawQueryResult
{
    public class UsuariosRawQuery
    {
        public Guid Codigo { get; set; }

        public Guid? CodigoParceiro { get; set; }

        public Situacao Situacao { get; set; }

        public string Nome { get; set; }

        public string Email { get; set; }

        public DateTime DtInclusao { get; set; }

        public DateTime DtSituacao { get; set; }

        public TipoPermissao TipoPermissao { get; set; }
    }
}
