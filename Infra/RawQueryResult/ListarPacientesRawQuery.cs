﻿using Domain.Enums;

namespace Infra.RawQueryResult
{
    public class ListarPacientesRawQuery
    {
        public Guid Codigo { get; set; }

        public Situacao Situacao { get; set; }

        public string Nome { get; set; }

        public DateTime DtInclusao { get; set; }

        public DateTime DtSituacao { get; set; }

        public string? DocumentoFederal { get; set; }

        public string? Telefone { get; set; }

        public string? Email { get; set; }
    }
}
