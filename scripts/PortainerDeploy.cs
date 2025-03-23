using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortainerDeploy
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Utilitário de Deploy para Portainer");
            Console.WriteLine("===================================");

            // Obter configurações do usuário
            Console.Write("URL do Portainer: ");
            string portainerUrl = Console.ReadLine();

            Console.Write("Usuário Portainer: ");
            string username = Console.ReadLine();

            Console.Write("Senha Portainer: ");
            string password = Console.ReadLine();

            Console.Write("Nome da imagem Docker (ex: usuario/auz-core-api:latest): ");
            string dockerImage = Console.ReadLine();

            Console.Write("Nome do container: ");
            string containerName = Console.ReadLine() ?? "auz-api";

            Console.Write("Porta do host (ex: 8080): ");
            string hostPort = Console.ReadLine() ?? "8080";

            try
            {
                await DeployContainer(
                    portainerUrl,
                    username,
                    password,
                    dockerImage,
                    containerName,
                    hostPort
                );
                Console.WriteLine("Deploy concluído com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante o deploy: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static async Task DeployContainer(
            string portainerUrl,
            string username,
            string password,
            string dockerImage,
            string containerName,
            string hostPort
        )
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(portainerUrl);

            Console.WriteLine("Autenticando no Portainer...");
            var authData = new { Username = username, Password = password };
            var authJson = JsonSerializer.Serialize(authData);
            var authContent = new StringContent(authJson, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/auth", authContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Falha na autenticação: {response.StatusCode}");
            }

            var authResponseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(
                authResponseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                authResponse.Jwt
            );

            Console.WriteLine("Procurando endpoints disponíveis...");
            var endpointsResponse = await client.GetAsync("/api/endpoints");
            var endpointsContent = await endpointsResponse.Content.ReadAsStringAsync();
            var endpoints = JsonSerializer.Deserialize<List<Endpoint>>(
                endpointsContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (endpoints == null || !endpoints.Any())
            {
                throw new Exception("Nenhum endpoint encontrado no Portainer");
            }

            // Usar o primeiro endpoint por padrão, ou permitir a seleção
            var endpoint = endpoints.First();
            Console.WriteLine($"Usando endpoint: {endpoint.Name} (ID: {endpoint.Id})");

            Console.WriteLine("Procurando containers existentes...");
            var containersResponse = await client.GetAsync(
                $"/api/endpoints/{endpoint.Id}/docker/containers/json?all=true"
            );
            var containersContent = await containersResponse.Content.ReadAsStringAsync();
            var containers = JsonSerializer.Deserialize<List<Container>>(
                containersContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var existingContainer = containers?.FirstOrDefault(c =>
                c.Names != null && c.Names.Any(n => n.Contains(containerName))
            );

            if (existingContainer != null)
            {
                Console.WriteLine($"Removendo container existente: {existingContainer.Id}");
                await client.DeleteAsync(
                    $"/api/endpoints/{endpoint.Id}/docker/containers/{existingContainer.Id}?force=true"
                );
            }

            Console.WriteLine("Criando novo container...");

            // Definição da configuração do container
            var portBindings = new Dictionary<string, object>();
            portBindings.Add("80/tcp", new[] { new { HostPort = hostPort } });

            var createData = new Dictionary<string, object>
            {
                ["Name"] = containerName,
                ["Image"] = dockerImage,
                ["ExposedPorts"] = new Dictionary<string, object> { { "80/tcp", new { } } },
                ["HostConfig"] = new Dictionary<string, object>
                {
                    ["PortBindings"] = portBindings,
                    ["RestartPolicy"] = new { Name = "always" },
                },
                ["Env"] = new[]
                {
                    "ASPNETCORE_ENVIRONMENT=Production",
                    "ASPNETCORE_URLS=http://+:80",
                },
            };

            var createJson = JsonSerializer.Serialize(createData);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await client.PostAsync(
                $"/api/endpoints/{endpoint.Id}/docker/containers/create?name={containerName}",
                createContent
            );

            if (!createResponse.IsSuccessStatusCode)
            {
                var error = await createResponse.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Falha na criação do container: {createResponse.StatusCode}\n{error}"
                );
            }

            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<CreateContainerResponse>(
                createResponseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            Console.WriteLine($"Iniciando container: {createResult.Id}");
            await client.PostAsync(
                $"/api/endpoints/{endpoint.Id}/docker/containers/{createResult.Id}/start",
                null
            );
        }
    }

    class AuthResponse
    {
        public string Jwt { get; set; }
    }

    class Endpoint
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class Container
    {
        public string Id { get; set; }
        public List<string> Names { get; set; }
    }

    class CreateContainerResponse
    {
        public string Id { get; set; }
    }
}
