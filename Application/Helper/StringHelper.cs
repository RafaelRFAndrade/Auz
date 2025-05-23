﻿using System.Text.RegularExpressions;

namespace Application.Helpers
{
    public static class StringHelper
    {
        public static bool ValidarCPF(this string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            cpf = Regex.Replace(cpf, "[^0-9]", ""); // Remove caracteres não numéricos

            if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false; // Verifica se tem 11 dígitos e não é repetido

            int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int primeiroDigito = CalcularDigito(tempCpf, multiplicadores1);
            int segundoDigito = CalcularDigito(tempCpf + primeiroDigito, multiplicadores2);

            return cpf.EndsWith($"{primeiroDigito}{segundoDigito}");
        }

        public static bool ValidarCelular(this string celular)
        {
            if (string.IsNullOrWhiteSpace(celular)) return false;

            celular = Regex.Replace(celular, "[^0-9]", "");

            if (celular.Length != 11) return false;

            if (celular[2] != '9') return false;

            string ddd = celular.Substring(0, 2);
            int dddNumero = int.Parse(ddd);
            if (dddNumero < 11 || dddNumero > 99) return false;

            return true;
        }

        private static int CalcularDigito(string cpf, int[] multiplicadores)
        {
            int soma = cpf.Select((t, i) => (t - '0') * multiplicadores[i]).Sum();
            int resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
    }
}
