using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortainerDeploy
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        private static string? authToken;
        private static bool debug = false;

        static async Task Main(string[] args)
        {
            try
            {
                bool interactive = args.Length > 0 && args[0] == "--interactive";
                debug = Environment.GetEnvironmentVariable("DEBUG")?.ToLower() == "true";

                string? portainerUrl = Environment.GetEnvironmentVariable("PORTAINER_URL");
                string? username = Environment.GetEnvironmentVariable("USERNAME");
                string? password = Environment.GetEnvironmentVariable("PASSWORD");
                string? dockerUsername = Environment.GetEnvironmentVariable("DOCKER_USERNAME");
                string? containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME");
                string? hostPort = Environment.GetEnvironmentVariable("HOST_PORT");

                if (interactive)
                {
                    Console.WriteLine("Executando em modo interativo.");

                    Console.Write("URL do Portainer (com /api no final): ");
                    portainerUrl = Console.ReadLine()?.Trim();

                    Console.Write("Nome de usuário do Portainer: ");
                    username = Console.ReadLine()?.Trim();

                    Console.Write("Senha do Portainer: ");
                    password = Console.ReadLine()?.Trim();

                    Console.Write("Nome de usuário do Docker Hub: ");
                    dockerUsername = Console.ReadLine()?.Trim();

                    Console.Write("Nome do container: ");
                    containerName = Console.ReadLine()?.Trim();

                    Console.Write("Porta do host (ex: 8080): ");
                    hostPort = Console.ReadLine()?.Trim();
                }
                else
                {
                    Console.WriteLine("Executando em modo CI/CD.");
                    if (debug)
                    {
                        Console.WriteLine($"PORTAINER_URL: {MaskSensitiveData(portainerUrl)}");
                        Console.WriteLine($"USERNAME: {MaskSensitiveData(username)}");
                        Console.WriteLine(
                            $"PASSWORD: {(password != null ? "definido" : "não definido")}"
                        );
                        Console.WriteLine($"DOCKER_USERNAME: {MaskSensitiveData(dockerUsername)}");
                        Console.WriteLine($"CONTAINER_NAME: {containerName}");
                        Console.WriteLine($"HOST_PORT: {hostPort}");
                    }
                }

                if (string.IsNullOrEmpty(portainerUrl))
                {
                    Console.WriteLine("Erro: URL do Portainer não fornecida.");
                    Environment.Exit(1);
                }

                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine("Erro: Nome de usuário do Portainer não fornecido.");
                    Environment.Exit(1);
                }

                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Erro: Senha do Portainer não fornecida.");
                    Environment.Exit(1);
                }

                if (string.IsNullOrEmpty(dockerUsername))
                {
                    Console.WriteLine("Erro: Nome de usuário do Docker Hub não fornecido.");
                    Environment.Exit(1);
                }

                if (string.IsNullOrEmpty(containerName))
                {
                    Console.WriteLine("Erro: Nome do container não fornecido.");
                    Environment.Exit(1);
                }

                if (string.IsNullOrEmpty(hostPort))
                {
                    Console.WriteLine("Erro: Porta do host não fornecida.");
                    Environment.Exit(1);
                }

                // Garante que a URL do Portainer termina com uma barra
                if (!portainerUrl.EndsWith("/"))
                {
                    portainerUrl += "/";
                }

                // Remove "api/" ou "api/v1/" ou "api/v2/" se já estiver incluído no URL
                if (portainerUrl.EndsWith("api/"))
                {
                    portainerUrl = portainerUrl.Substring(0, portainerUrl.Length - 4);
                }
                else if (portainerUrl.EndsWith("api/v1/") || portainerUrl.EndsWith("api/v2/"))
                {
                    portainerUrl = portainerUrl.Substring(0, portainerUrl.Length - 7);
                }

                Console.WriteLine("Iniciando processo de deploy no Portainer...");

                string imageName = $"{dockerUsername}/{containerName}:latest";

                // Configura o cliente HTTP
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );

                await AutenticarPortainer(portainerUrl, username, password);

                int endpointId = await ObterEndpointId(portainerUrl);
                if (endpointId == -1)
                {
                    Console.WriteLine(
                        "Erro: Não foi possível obter o ID do endpoint do Portainer."
                    );
                    Environment.Exit(1);
                }

                await DeployContainer(portainerUrl, endpointId, imageName, containerName, hostPort);

                Console.WriteLine("Deploy realizado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante o deploy: {ex.Message}");
                if (debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
                Environment.Exit(1);
            }
        }

        private static string MaskSensitiveData(string? data)
        {
            if (string.IsNullOrEmpty(data))
                return "não definido";

            if (data.Length <= 4)
                return "***";

            return data.Substring(0, 3) + "..." + data.Substring(data.Length - 2, 2);
        }

        private static async Task AutenticarPortainer(
            string baseUrl,
            string username,
            string password
        )
        {
            Console.WriteLine("Autenticando no Portainer...");

            var loginUrl = $"{baseUrl}api/auth";
            if (debug)
                Console.WriteLine($"URL de autenticação: {loginUrl}");

            var loginData = new { Username = username, Password = password };

            var response = await client.PostAsJsonAsync(loginUrl, loginData);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Falha na autenticação. Status: {response.StatusCode}");
                Console.WriteLine($"Resposta: {errorContent}");
                throw new Exception($"Falha na autenticação do Portainer: {response.StatusCode}");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            authToken = authResponse?.jwt;

            if (string.IsNullOrEmpty(authToken))
            {
                throw new Exception("Token de autenticação não recebido");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                authToken
            );
            Console.WriteLine("Autenticação bem-sucedida!");
        }

        private static async Task<int> ObterEndpointId(string baseUrl)
        {
            Console.WriteLine("Obtendo endpoints do Portainer...");

            var endpointsUrl = $"{baseUrl}api/endpoints";
            if (debug)
                Console.WriteLine($"URL dos endpoints: {endpointsUrl}");

            // Tenta primeiro a API v2
            var response = await client.GetAsync(endpointsUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Falha ao obter endpoints. Status: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Resposta: {errorContent}");
                throw new Exception(
                    $"Não foi possível obter endpoints do Portainer: {response.StatusCode}"
                );
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            if (debug)
                Console.WriteLine($"Resposta de endpoints: {responseBody}");

            // Verificar se a resposta é um array vazio
            if (responseBody == "[]")
            {
                Console.WriteLine("Array de endpoints vazio retornado pelo Portainer.");
                Console.WriteLine("Tentando usar o endpoint padrão (ID: 1)...");

                // Verificar se o endpoint 1 existe
                var endpointCheckUrl = $"{baseUrl}api/endpoints/1";
                if (debug)
                    Console.WriteLine($"Verificando endpoint 1: {endpointCheckUrl}");

                var endpointCheckResponse = await client.GetAsync(endpointCheckUrl);
                if (endpointCheckResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Endpoint 1 encontrado e será usado para o deploy.");
                    return 1;
                }
                else
                {
                    Console.WriteLine(
                        $"Endpoint 1 não disponível. Status: {endpointCheckResponse.StatusCode}"
                    );

                    // Como último recurso, tentar listar todos os containers sem especificar endpoint
                    var containersCheckUrl = $"{baseUrl}api/docker/containers/json?all=true";
                    if (debug)
                        Console.WriteLine(
                            $"Verificando acesso direto a containers: {containersCheckUrl}"
                        );

                    var containersCheckResponse = await client.GetAsync(containersCheckUrl);
                    if (containersCheckResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine(
                            "Endpoint não é necessário nesta versão do Portainer. Usando ID 0."
                        );
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine(
                            "Não foi possível encontrar um endpoint válido no Portainer."
                        );
                        return -1;
                    }
                }
            }

            // Tenta desserializar como um array de endpoints primeiro (API comum)
            Endpoint[]? endpoints = null;
            try
            {
                endpoints = JsonSerializer.Deserialize<Endpoint[]>(responseBody, jsonOptions);
                if (debug)
                    Console.WriteLine("Conseguiu desserializar como array de endpoints");
            }
            catch (JsonException)
            {
                if (debug)
                    Console.WriteLine("Falha ao desserializar como array, tentando outro formato");
            }

            // Se não conseguiu como array, tenta como objeto EndpointResponse
            if (endpoints == null || endpoints.Length == 0)
            {
                try
                {
                    var endpointResponse = JsonSerializer.Deserialize<EndpointResponse>(
                        responseBody,
                        jsonOptions
                    );
                    endpoints = endpointResponse?.endpoints?.ToArray();
                    if (debug)
                        Console.WriteLine("Conseguiu desserializar como EndpointResponse");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Erro ao desserializar resposta de endpoints: {ex.Message}");
                    if (debug)
                        Console.WriteLine($"Conteúdo que falhou: {responseBody}");

                    // Tentar usar endpoint padrão como último recurso
                    Console.WriteLine("Tentando usar o endpoint padrão (ID: 1)...");
                    return 1;
                }
            }

            if (endpoints == null || endpoints.Length == 0)
            {
                Console.WriteLine("Nenhum endpoint encontrado no Portainer.");
                Console.WriteLine("Tentando usar o endpoint padrão (ID: 1)...");
                return 1;
            }

            Console.WriteLine($"Endpoints encontrados: {endpoints.Length}");
            foreach (var endpoint in endpoints)
            {
                Console.WriteLine(
                    $"Endpoint ID: {endpoint.Id}, Nome: {endpoint.Name}, URL: {endpoint.URL}"
                );
            }

            // Usa o primeiro endpoint por padrão
            int endpointId = endpoints[0].Id;
            Console.WriteLine($"Usando endpoint: {endpoints[0].Name} (ID: {endpointId})");
            return endpointId;
        }

        private static async Task DeployContainer(
            string baseUrl,
            int endpointId,
            string imageName,
            string containerName,
            string hostPort
        )
        {
            Console.WriteLine($"Iniciando deploy do container {containerName}...");

            // Constrói o caminho base para as chamadas Docker, considerando se o endpoint é necessário
            string dockerBasePath;
            if (endpointId > 0)
            {
                dockerBasePath = $"{baseUrl}api/endpoints/{endpointId}/docker";
            }
            else
            {
                dockerBasePath = $"{baseUrl}api/docker";
            }

            // 1. Verificar se o container existe e removê-lo
            await RemoverContainerSeExistir(baseUrl, endpointId, containerName);

            // 2. Puxar a nova imagem
            await PuxarImagem(baseUrl, endpointId, imageName);

            // 3. Criar e iniciar o novo container
            await CriarEIniciarContainer(baseUrl, endpointId, imageName, containerName, hostPort);
        }

        private static async Task RemoverContainerSeExistir(
            string baseUrl,
            int endpointId,
            string containerName
        )
        {
            Console.WriteLine("Verificando se o container já existe...");

            string containersUrl;
            if (endpointId > 0)
            {
                containersUrl =
                    $"{baseUrl}api/endpoints/{endpointId}/docker/containers/json?all=true";
            }
            else
            {
                containersUrl = $"{baseUrl}api/docker/containers/json?all=true";
            }

            if (debug)
                Console.WriteLine($"URL de consulta de containers: {containersUrl}");

            var response = await client.GetAsync(containersUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(
                    $"Falha ao obter lista de containers. Status: {response.StatusCode}, Resposta: {errorContent}"
                );
                throw new Exception(
                    $"Não foi possível obter a lista de containers: {response.StatusCode}"
                );
            }

            var containers = await response.Content.ReadFromJsonAsync<Container[]>();

            var container = containers?.FirstOrDefault(c =>
                c.Names?.Any(n => n.Contains(containerName, StringComparison.OrdinalIgnoreCase))
                == true
            );

            if (container != null)
            {
                Console.WriteLine($"Container existente encontrado. ID: {container.Id}");

                // Parar o container
                Console.WriteLine("Parando o container...");
                string stopUrl;
                if (endpointId > 0)
                {
                    stopUrl =
                        $"{baseUrl}api/endpoints/{endpointId}/docker/containers/{container.Id}/stop";
                }
                else
                {
                    stopUrl = $"{baseUrl}api/docker/containers/{container.Id}/stop";
                }

                var stopResponse = await client.PostAsync(stopUrl, null);

                if (
                    !stopResponse.IsSuccessStatusCode
                    && stopResponse.StatusCode != System.Net.HttpStatusCode.NotModified
                )
                {
                    var errorContent = await stopResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(
                        $"Aviso: Falha ao parar o container. Status: {stopResponse.StatusCode}, Resposta: {errorContent}"
                    );
                    // Continua mesmo se falhar para tentar remover de qualquer forma
                }

                // Remover o container
                Console.WriteLine("Removendo o container...");
                string removeUrl;
                if (endpointId > 0)
                {
                    removeUrl =
                        $"{baseUrl}api/endpoints/{endpointId}/docker/containers/{container.Id}?force=true";
                }
                else
                {
                    removeUrl = $"{baseUrl}api/docker/containers/{container.Id}?force=true";
                }

                var removeResponse = await client.DeleteAsync(removeUrl);

                if (!removeResponse.IsSuccessStatusCode)
                {
                    var errorContent = await removeResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(
                        $"Falha ao remover o container. Status: {removeResponse.StatusCode}, Resposta: {errorContent}"
                    );
                    throw new Exception("Não foi possível remover o container existente");
                }

                Console.WriteLine("Container removido com sucesso.");
            }
            else
            {
                Console.WriteLine("Container não encontrado. Prosseguindo com a criação.");
            }
        }

        private static async Task PuxarImagem(string baseUrl, int endpointId, string imageName)
        {
            Console.WriteLine($"Puxando imagem {imageName}...");

            string pullUrl;
            if (endpointId > 0)
            {
                pullUrl =
                    $"{baseUrl}api/endpoints/{endpointId}/docker/images/create?fromImage={imageName}";
            }
            else
            {
                pullUrl = $"{baseUrl}api/docker/images/create?fromImage={imageName}";
            }

            if (debug)
                Console.WriteLine($"URL para pull da imagem: {pullUrl}");

            var response = await client.PostAsync(pullUrl, null);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(
                    $"Falha ao puxar a imagem. Status: {response.StatusCode}, Resposta: {errorContent}"
                );
                throw new Exception($"Não foi possível puxar a imagem {imageName}");
            }

            Console.WriteLine("Imagem puxada com sucesso.");
        }

        private static async Task CriarEIniciarContainer(
            string baseUrl,
            int endpointId,
            string imageName,
            string containerName,
            string hostPort
        )
        {
            Console.WriteLine("Criando novo container...");

            string createUrl;
            if (endpointId > 0)
            {
                createUrl =
                    $"{baseUrl}api/endpoints/{endpointId}/docker/containers/create?name={containerName}";
            }
            else
            {
                createUrl = $"{baseUrl}api/docker/containers/create?name={containerName}";
            }

            if (debug)
                Console.WriteLine($"URL para criação do container: {createUrl}");

            var containerConfig = new
            {
                Image = imageName,
                HostConfig = new
                {
                    PortBindings = new Dictionary<string, object>
                    {
                        { "80/tcp", new[] { new { HostPort = hostPort } } },
                    },
                    RestartPolicy = new { Name = "always" },
                },
                ExposedPorts = new Dictionary<string, object> { { "80/tcp", new { } } },
            };

            if (debug)
                Console.WriteLine(
                    $"Configuração do container: {JsonSerializer.Serialize(containerConfig, jsonOptions)}"
                );

            var jsonContent = JsonSerializer.Serialize(containerConfig);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(createUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(
                    $"Falha ao criar o container. Status: {response.StatusCode}, Resposta: {errorContent}"
                );
                throw new Exception("Não foi possível criar o container");
            }

            var createResponse =
                await response.Content.ReadFromJsonAsync<CreateContainerResponse>();
            string containerId =
                createResponse?.Id ?? throw new Exception("ID do container não recebido");

            Console.WriteLine($"Container criado com ID: {containerId}");

            // Iniciar o container
            Console.WriteLine("Iniciando o container...");
            string startUrl;
            if (endpointId > 0)
            {
                startUrl =
                    $"{baseUrl}api/endpoints/{endpointId}/docker/containers/{containerId}/start";
            }
            else
            {
                startUrl = $"{baseUrl}api/docker/containers/{containerId}/start";
            }

            var startResponse = await client.PostAsync(startUrl, null);

            if (!startResponse.IsSuccessStatusCode)
            {
                var errorContent = await startResponse.Content.ReadAsStringAsync();
                Console.WriteLine(
                    $"Falha ao iniciar o container. Status: {startResponse.StatusCode}, Resposta: {errorContent}"
                );
                throw new Exception("Não foi possível iniciar o container");
            }

            Console.WriteLine("Container iniciado com sucesso!");
        }

        // Classes para desserialização
        class AuthResponse
        {
            public string? jwt { get; set; }
        }

        class EndpointResponse
        {
            public List<Endpoint>? endpoints { get; set; }
        }

        class Endpoint
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? URL { get; set; }
            public int Status { get; set; }
            public string? Type { get; set; }
        }

        class Container
        {
            public string? Id { get; set; }
            public string? Image { get; set; }
            public string[]? Names { get; set; }
            public string? State { get; set; }
        }

        class CreateContainerResponse
        {
            public string? Id { get; set; }
            public string[]? Warnings { get; set; }
        }
    }
}
