using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PortainerDeploy
{
    class Program
    {
        private static readonly HttpClient client;
        private static readonly CookieContainer cookieContainer;
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        private static string? authToken;
        private static string? csrfToken;
        private static bool usandoSessao = false;
        private static bool debug = false;

        static Program()
        {
            // Inicializa o HttpClient com suporte a cookies
            cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AllowAutoRedirect = true,
            };
            client = new HttpClient(handler);
        }

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
                
                // Verificar a edição do Portainer para ajustar a estratégia
                string? edicaoPortainer = await VerificarEdicaoPortainer(portainerUrl);
                
                // Tratamento especial para Portainer CE
                if (edicaoPortainer == "CE")
                {
                    Console.WriteLine("Usando tratamento especial para Portainer CE");
                    // O Portainer CE pode ter requisitos específicos para CSRF
                    // que implementamos nas funções de renovação
                }

                int endpointId = await ObterEndpointId(portainerUrl);
                if (endpointId == -1)
                {
                    Console.WriteLine(
                        "Aviso: Não foi possível detectar automaticamente o endpoint do Portainer. Tentando usar o endpoint 3."
                    );
                    endpointId = 3;
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

            // Verificar se precisamos de token CSRF
            bool usaCsrf = false;

            // Tentar obter o token CSRF primeiro
            try
            {
                var csrfUrl = $"{baseUrl}";
                if (debug)
                    Console.WriteLine($"Verificando token CSRF: {csrfUrl}");

                // Faz uma requisição GET inicial para obter cookies e potencialmente o token CSRF
                var csrfResponse = await client.GetAsync(csrfUrl);
                
                // Exibir todos os cabeçalhos da resposta para debug
                if (debug)
                {
                    Console.WriteLine("Todos os cabeçalhos da resposta inicial:");
                    foreach (var header in csrfResponse.Headers)
                    {
                        Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                    }
                }

                // Verifica se há cabeçalhos de CSRF na resposta
                if (csrfResponse.Headers.TryGetValues("X-CSRF-Token", out var csrfValues))
                {
                    csrfToken = csrfValues.FirstOrDefault();
                    usaCsrf = true;
                    Console.WriteLine($"Token CSRF encontrado no cabeçalho X-CSRF-Token: {csrfToken}");
                }
                else if (csrfResponse.Headers.TryGetValues("X-XSRF-Token", out var xsrfValues))
                {
                    csrfToken = xsrfValues.FirstOrDefault();
                    usaCsrf = true;
                    Console.WriteLine($"Token CSRF encontrado no cabeçalho X-XSRF-Token: {csrfToken}");
                }
                else if (csrfResponse.Headers.TryGetValues("X-Portainer-CSRF", out var portainerCsrfValues))
                {
                    csrfToken = portainerCsrfValues.FirstOrDefault();
                    usaCsrf = true;
                    Console.WriteLine($"Token CSRF encontrado no cabeçalho X-Portainer-CSRF: {csrfToken}");
                }

                // Se não encontrou no cabeçalho, procura nos cookies
                if (string.IsNullOrEmpty(csrfToken))
                {
                    var cookies = cookieContainer.GetCookies(new Uri(baseUrl));
                    Console.WriteLine("Cookies recebidos na resposta inicial:");
                    foreach (Cookie cookie in cookies)
                    {
                        Console.WriteLine($"  {cookie.Name}: {cookie.Value}");
                        
                        if (
                            cookie.Name.Contains("csrf", StringComparison.OrdinalIgnoreCase) ||
                            cookie.Name.Contains("XSRF", StringComparison.OrdinalIgnoreCase) ||
                            cookie.Name.Contains("portainer.CSRF", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            csrfToken = cookie.Value;
                            usaCsrf = true;
                            Console.WriteLine($"Token CSRF encontrado no cookie {cookie.Name}: {csrfToken}");
                            break;
                        }
                    }
                }

                // Se ainda não encontrou, tenta verificar se há um token na resposta HTML
                if (string.IsNullOrEmpty(csrfToken))
                {
                    var htmlContent = await csrfResponse.Content.ReadAsStringAsync();
                    csrfToken = ExtrairTokenCsrfDoHtml(htmlContent);
                    
                    if (!string.IsNullOrEmpty(csrfToken))
                    {
                        usaCsrf = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug)
                    Console.WriteLine($"Erro ao obter token CSRF: {ex.Message}");
                // Continuamos mesmo sem o token, tentando a autenticação direta
            }

            // Tentar primeiro a autenticação com JWT
            try
            {
                var loginUrl = $"{baseUrl}api/auth";
                if (debug)
                    Console.WriteLine($"URL de autenticação JWT: {loginUrl}");

                var loginData = new { Username = username, Password = password };
                var jsonContent = JsonSerializer.Serialize(loginData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Adicionar cabeçalho CSRF se necessário
                var request = new HttpRequestMessage(HttpMethod.Post, loginUrl)
                {
                    Content = content,
                };

                if (usaCsrf && !string.IsNullOrEmpty(csrfToken))
                {
                    request.Headers.Add("X-CSRF-Token", csrfToken);
                    if (debug)
                        Console.WriteLine("Adicionado cabeçalho X-CSRF-Token à requisição JWT");
                }

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    authToken = authResponse?.jwt;

                    if (!string.IsNullOrEmpty(authToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                            "Bearer",
                            authToken
                        );
                        Console.WriteLine("Autenticação JWT bem-sucedida!");
                        
                        // Verificar se a resposta de autenticação contém um token CSRF
                        if (response.Headers.TryGetValues("X-CSRF-Token", out var authCsrfValues))
                        {
                            csrfToken = authCsrfValues.FirstOrDefault();
                            usaCsrf = true;
                            Console.WriteLine($"Token CSRF encontrado no cabeçalho da resposta de autenticação: {csrfToken}");
                        }
                        else if (response.Headers.TryGetValues("X-XSRF-Token", out var authXsrfValues))
                        {
                            csrfToken = authXsrfValues.FirstOrDefault();
                            usaCsrf = true;
                            Console.WriteLine($"Token CSRF encontrado como X-XSRF-Token na resposta de autenticação: {csrfToken}");
                        }
                        
                        // Verificar cookies após autenticação
                        var authCookies = cookieContainer.GetCookies(new Uri(baseUrl));
                        if (debug)
                        {
                            Console.WriteLine("Cookies após autenticação JWT:");
                            foreach (Cookie cookie in authCookies)
                            {
                                Console.WriteLine($"  {cookie.Name}: {cookie.Value}");
                                
                                if (cookie.Name.Contains("csrf", StringComparison.OrdinalIgnoreCase) || 
                                    cookie.Name.Contains("xsrf", StringComparison.OrdinalIgnoreCase))
                                {
                                    csrfToken = cookie.Value;
                                    usaCsrf = true;
                                    Console.WriteLine($"Token CSRF encontrado em cookie após autenticação: {csrfToken}");
                                }
                            }
                        }
                        
                        // Se ainda não temos um CSRF token, tentar obter da página inicial
                        if (string.IsNullOrEmpty(csrfToken))
                        {
                            Console.WriteLine("Tentando obter token CSRF após autenticação JWT...");
                            try
                            {
                                var postAuthResponse = await client.GetAsync(baseUrl);
                                if (postAuthResponse.IsSuccessStatusCode)
                                {
                                    // Verificar headers
                                    if (postAuthResponse.Headers.TryGetValues("X-CSRF-Token", out var postAuthCsrfValues))
                                    {
                                        csrfToken = postAuthCsrfValues.FirstOrDefault();
                                        usaCsrf = true;
                                        Console.WriteLine($"Token CSRF encontrado após autenticação JWT: {csrfToken}");
                                    }
                                    
                                    // Verificar o HTML
                                    var postAuthHtml = await postAuthResponse.Content.ReadAsStringAsync();
                                    var extractedToken = ExtrairTokenCsrfDoHtml(postAuthHtml);
                                    if (!string.IsNullOrEmpty(extractedToken))
                                    {
                                        csrfToken = extractedToken;
                                        usaCsrf = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug)
                                    Console.WriteLine($"Erro ao tentar obter token CSRF após autenticação: {ex.Message}");
                            }
                        }
                        
                        // Se ainda não temos um token CSRF, vamos criar um para testes
                        if (string.IsNullOrEmpty(csrfToken))
                        {
                            // Estratégia alternativa: usar um GUID como token CSRF para testes
                            csrfToken = Guid.NewGuid().ToString("N");
                            usaCsrf = true;
                            Console.WriteLine($"Criado token CSRF para testes: {csrfToken}");
                        }
                        
                        return;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (debug)
                    {
                        Console.WriteLine(
                            $"Falha na autenticação JWT. Status: {response.StatusCode}"
                        );
                        Console.WriteLine($"Resposta: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug)
                    Console.WriteLine($"Erro na autenticação JWT: {ex.Message}");
            }

            // Se a autenticação JWT falhar, tentar com autenticação baseada em sessão
            Console.WriteLine("Tentando autenticação baseada em sessão...");
            try
            {
                var formLoginUrl = $"{baseUrl}";
                if (debug)
                    Console.WriteLine($"URL de login por formulário: {formLoginUrl}");

                // Verificar se há um formulário de login
                var loginPageResponse = await client.GetAsync(formLoginUrl);
                var loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();

                // Extrair a URL do formulário de login se estiver presente
                string formAction = formLoginUrl;
                var formMatch = Regex.Match(
                    loginPageContent,
                    @"<form[^>]*action=[""']([^""']+)[""']",
                    RegexOptions.IgnoreCase
                );
                if (formMatch.Success)
                {
                    var extractedAction = formMatch.Groups[1].Value;

                    // Se a URL for relativa, combiná-la com a URL base
                    if (extractedAction.StartsWith("/"))
                    {
                        Uri baseUri = new Uri(baseUrl);
                        formAction = $"{baseUri.Scheme}://{baseUri.Authority}{extractedAction}";
                    }
                    else if (!extractedAction.StartsWith("http"))
                    {
                        formAction = $"{baseUrl.TrimEnd('/')}/{extractedAction.TrimStart('/')}";
                    }
                    else
                    {
                        formAction = extractedAction;
                    }

                    if (debug)
                        Console.WriteLine(
                            $"Formulário de login encontrado com action: {formAction}"
                        );
                }

                // Se o CSRF ainda não foi encontrado, procurar no formulário
                if (string.IsNullOrEmpty(csrfToken))
                {
                    var csrfMatch = Regex.Match(
                        loginPageContent,
                        @"<input[^>]*name=[""']_csrf[""'][^>]*value=[""']([^""']+)[""']",
                        RegexOptions.IgnoreCase
                    );
                    if (csrfMatch.Success)
                    {
                        csrfToken = csrfMatch.Groups[1].Value;
                        usaCsrf = true;
                        if (debug)
                            Console.WriteLine($"Token CSRF encontrado no formulário: {csrfToken}");
                    }
                }

                // Preparar os dados do formulário
                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                };

                // Adicionar token CSRF se necessário
                if (usaCsrf && !string.IsNullOrEmpty(csrfToken))
                {
                    formData.Add(new KeyValuePair<string, string>("_csrf", csrfToken));
                }

                var formContent = new FormUrlEncodedContent(formData);

                // Criar a requisição POST para o formulário
                var formRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
                {
                    Content = formContent,
                };

                // Adicionar headers necessários
                formRequest.Headers.Add("User-Agent", "Mozilla/5.0");
                formRequest.Headers.Add(
                    "Accept",
                    "text/html,application/xhtml+xml,application/xml"
                );
                if (usaCsrf && !string.IsNullOrEmpty(csrfToken))
                {
                    formRequest.Headers.Add("X-CSRF-Token", csrfToken);
                }

                // Enviar a requisição
                var formResponse = await client.SendAsync(formRequest);

                // Verificar o resultado
                if (
                    formResponse.IsSuccessStatusCode
                    || formResponse.StatusCode == HttpStatusCode.Found
                )
                {
                    // Verificar se temos cookies de sessão
                    var cookies = cookieContainer.GetCookies(new Uri(baseUrl));
                    bool temSessao = false;

                    foreach (Cookie cookie in cookies)
                    {
                        if (
                            cookie.Name.Contains("session", StringComparison.OrdinalIgnoreCase)
                            || cookie.Name.Contains("portainer", StringComparison.OrdinalIgnoreCase)
                            || cookie.Name.Contains("auth", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            if (debug)
                                Console.WriteLine($"Cookie de sessão encontrado: {cookie.Name}");
                            temSessao = true;
                            break;
                        }
                    }

                    if (temSessao)
                    {
                        Console.WriteLine("Autenticação baseada em sessão bem-sucedida!");
                        usandoSessao = true;
                        return;
                    }
                }
                else
                {
                    var errorContent = await formResponse.Content.ReadAsStringAsync();
                    if (debug)
                    {
                        Console.WriteLine(
                            $"Falha na autenticação por formulário. Status: {formResponse.StatusCode}"
                        );
                        Console.WriteLine($"Resposta: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug)
                    Console.WriteLine($"Erro na autenticação por formulário: {ex.Message}");
            }

            // Se chegamos aqui, nenhum método de autenticação funcionou
            throw new Exception(
                "Não foi possível autenticar no Portainer. Ambos os métodos (JWT e sessão) falharam."
            );
        }

        private static async Task<int> ObterEndpointId(string baseUrl)
        {
            Console.WriteLine("Obtendo endpoints do Portainer...");

            // Tentar diretamente o endpoint 3 que descobrimos manualmente
            try
            {
                var testEndpoint3Url = $"{baseUrl}api/endpoints/3/docker/containers/json?all=true";
                Console.WriteLine($"Testando diretamente o endpoint 3: {testEndpoint3Url}");
                
                // Usar o método auxiliar para testar o endpoint 3 com token CSRF
                var requisicao = CriarRequisicaoComCsrf(HttpMethod.Get, testEndpoint3Url);
                var testResponse = await client.SendAsync(requisicao);
                
                if (testResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Endpoint 3 está disponível e funcionando! Usando-o diretamente.");
                    return 3;
                }
                else
                {
                    if (debug)
                        Console.WriteLine($"Endpoint 3 não respondeu com sucesso: {testResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                if (debug)
                    Console.WriteLine($"Erro ao testar endpoint 3: {ex.Message}");
            }

            // Se estamos usando sessão, tentar obter o endpoint da página inicial
            if (usandoSessao)
            {
                try
                {
                    Console.WriteLine(
                        "Usando autenticação baseada em sessão, tentando determinar endpoint a partir da interface..."
                    );
                    var homeUrl = $"{baseUrl}";
                    if (debug)
                        Console.WriteLine($"Acessando página inicial: {homeUrl}");

                    var homeRequisicao = CriarRequisicaoComCsrf(HttpMethod.Get, homeUrl);
                    var homeResponse = await client.SendAsync(homeRequisicao);
                    
                    if (homeResponse.IsSuccessStatusCode)
                    {
                        var homeContent = await homeResponse.Content.ReadAsStringAsync();

                        // Procurar por padrões comuns de referência a endpoints
                        var endpointMatches = new[]
                        {
                            // Tenta encontrar um padrão onde ID de endpoint é mencionado
                            Regex.Match(homeContent, @"endpoint[/=](\d+)", RegexOptions.IgnoreCase),
                            Regex.Match(
                                homeContent,
                                @"endpointId['""\s:=]+(\d+)",
                                RegexOptions.IgnoreCase
                            ),
                            Regex.Match(
                                homeContent,
                                @"endpoint_id['""\s:=]+(\d+)",
                                RegexOptions.IgnoreCase
                            ),
                        };

                        foreach (var match in endpointMatches)
                        {
                            if (
                                match.Success
                                && int.TryParse(match.Groups[1].Value, out int matchedEndpointId)
                            )
                            {
                                Console.WriteLine(
                                    $"Endpoint ID {matchedEndpointId} encontrado na interface web."
                                );
                                return matchedEndpointId;
                            }
                        }

                        // Se não encontrou ID específico, tentar ver se há uma lista de containers diretamente
                        if (
                            homeContent.Contains("/docker/containers")
                            || homeContent.Contains("DockerContainerController")
                        )
                        {
                            Console.WriteLine(
                                "Interface do Portainer detectada com acesso direto a containers, sem endpoint."
                            );
                            return -1;
                        }
                    }

                    // Tentar com o endpoint padrão
                    Console.WriteLine(
                        "Não foi possível determinar o endpoint a partir da interface web. Usando endpoint padrão (1)."
                    );
                    return 1;
                }
                catch (Exception ex)
                {
                    if (debug)
                        Console.WriteLine(
                            $"Erro ao determinar endpoint via interface: {ex.Message}"
                        );
                    // Continuar com o método padrão
                }
            }

            var endpointsUrl = $"{baseUrl}api/endpoints";
            if (debug)
                Console.WriteLine($"URL dos endpoints: {endpointsUrl}");

            // Tenta primeiro a API v2
            var endpointsRequisicao = CriarRequisicaoComCsrf(HttpMethod.Get, endpointsUrl);
            var response = await client.SendAsync(endpointsRequisicao);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Falha ao obter endpoints. Status: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Resposta: {errorContent}");

                // Se estamos usando sessão, podemos tentar outra abordagem
                if (usandoSessao)
                {
                    Console.WriteLine(
                        "Usando autenticação baseada em sessão, tentando endpoint padrão."
                    );
                    return 1;
                }

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

                // Tentar descobrir onde está o Docker API no Portainer
                var containerApiUrls = new[]
                {
                    $"{baseUrl}api/docker/containers/json?all=true", // Portainer padrão sem endpoint
                    $"{baseUrl}api/v1/docker/containers/json?all=true", // Tentar versão API v1
                    $"{baseUrl}api/v2/docker/containers/json?all=true", // Tentar versão API v2
                    $"{baseUrl}docker/containers/json?all=true", // Tentar sem o "api/"
                    $"{baseUrl}api/local/docker/containers/json?all=true", // Tentar com endpoint "local"
                    $"{baseUrl}api/endpoints/1/docker/containers/json?all=true", // Tentar com endpoint 1
                    $"{baseUrl}api/endpoints/2/docker/containers/json?all=true", // Tentar com endpoint 2
                    $"{baseUrl}api/endpoints/3/docker/containers/json?all=true" // Tentar com endpoint 3
                };

                for (int i = 0; i < containerApiUrls.Length; i++)
                {
                    var url = containerApiUrls[i];
                    if (debug)
                        Console.WriteLine($"Tentando URL alternativa para Docker API: {url}");

                    try
                    {
                        var containerCheckRequisicao = CriarRequisicaoComCsrf(HttpMethod.Get, url);
                        var containerCheckResponse = await client.SendAsync(containerCheckRequisicao);
                        
                        if (containerCheckResponse.IsSuccessStatusCode)
                        {
                            // Detectar qual padrão funcionou
                            if (url.Contains("/endpoints/1/"))
                            {
                                Console.WriteLine("Endpoint 1 disponível para o Docker API.");
                                return 1; // Endpoint 1
                            }
                            else if (url.Contains("/endpoints/2/"))
                            {
                                Console.WriteLine("Endpoint 2 disponível para o Docker API.");
                                return 2; // Endpoint 2
                            }
                            else if (url.Contains("/endpoints/3/"))
                            {
                                Console.WriteLine("Endpoint 3 disponível para o Docker API.");
                                return 3; // Endpoint 3
                            }
                            else if (url.Contains("/local/"))
                            {
                                Console.WriteLine("Usando endpoint 'local' para o Docker API.");
                                return -2; // Código especial para endpoint "local"
                            }
                            else
                            {
                                Console.WriteLine("Usando Docker API diretamente, sem endpoint.");
                                return -1; // Código especial para Docker direto sem endpoint
                            }
                        }
                        else
                        {
                            var statusCode = containerCheckResponse.StatusCode;
                            if (debug)
                                Console.WriteLine($"URL {url} falhou com status: {statusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debug)
                            Console.WriteLine($"Erro ao tentar URL {url}: {ex.Message}");
                    }
                }

                // Se chegarmos aqui, todas as tentativas falharam
                // Tentar uma última abordagem - usar uma URL especial de status para detectar o tipo de Portainer
                var statusUrl = $"{baseUrl}api/status";
                if (debug)
                    Console.WriteLine($"Tentando verificar o status do Portainer: {statusUrl}");

                try
                {
                    var statusRequisicao = CriarRequisicaoComCsrf(HttpMethod.Get, statusUrl);
                    var statusResponse = await client.SendAsync(statusRequisicao);
                    
                    if (statusResponse.IsSuccessStatusCode)
                    {
                        var statusContent = await statusResponse.Content.ReadAsStringAsync();
                        if (debug)
                            Console.WriteLine($"Resposta de status: {statusContent}");

                        // Verifica se é uma versão Edge do Portainer
                        if (
                            statusContent.Contains("\"Edition\"")
                            && statusContent.Contains("\"Edge\"")
                        )
                        {
                            Console.WriteLine(
                                "Detectada instalação Portainer Edge. Tentando usar endpoint 'edge'."
                            );
                            return -3; // Código especial para edge
                        }
                    }
                }
                catch { }

                // Última tentativa - forçar usar -1 como "sem endpoint"
                Console.WriteLine(
                    "Nenhum endpoint encontrado. Tentando usar o endpoint com ID 3 (encontrado anteriormente nas requisições manuais)."
                );
                return 3; // Usar o endpoint 3 que sabemos que existe
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
                    Console.WriteLine("Tentando usar o endpoint 3...");
                    return 3;
                }
            }

            if (endpoints == null || endpoints.Length == 0)
            {
                Console.WriteLine("Nenhum endpoint encontrado no Portainer.");
                Console.WriteLine("Tentando usar o endpoint 3...");
                return 3;
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

        private static async Task<bool> RenovarTokenCsrf(string baseUrl)
        {
            Console.WriteLine("Renovando token CSRF...");
            try
            {
                // Fazer uma solicitação GET para a página inicial para obter um novo token CSRF
                var csrfResponse = await client.GetAsync(baseUrl);
                
                if (csrfResponse.IsSuccessStatusCode)
                {
                    // Verificar headers para token CSRF
                    if (csrfResponse.Headers.TryGetValues("X-CSRF-Token", out var csrfValues))
                    {
                        csrfToken = csrfValues.FirstOrDefault();
                        Console.WriteLine($"Token CSRF renovado a partir do header: {csrfToken}");
                        return true;
                    }
                    
                    // Verificar cookies
                    var cookies = cookieContainer.GetCookies(new Uri(baseUrl));
                    foreach (Cookie cookie in cookies)
                    {
                        if (cookie.Name.Contains("csrf", StringComparison.OrdinalIgnoreCase) || 
                            cookie.Name.Contains("xsrf", StringComparison.OrdinalIgnoreCase))
                        {
                            csrfToken = cookie.Value;
                            Console.WriteLine($"Token CSRF renovado a partir de cookie: {csrfToken}");
                            return true;
                        }
                    }
                    
                    // Extrair do HTML
                    var htmlContent = await csrfResponse.Content.ReadAsStringAsync();
                    var extractedToken = ExtrairTokenCsrfDoHtml(htmlContent);
                    if (!string.IsNullOrEmpty(extractedToken))
                    {
                        csrfToken = extractedToken;
                        Console.WriteLine($"Token CSRF renovado a partir do HTML: {csrfToken}");
                        return true;
                    }
                    
                    Console.WriteLine("Não foi possível renovar o token CSRF - nenhum token encontrado");
                    return false;
                }
                else
                {
                    Console.WriteLine($"Falha ao renovar token CSRF. Status: {csrfResponse.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao renovar token CSRF: {ex.Message}");
                return false;
            }
        }

        private static async Task DeployContainer(
            string baseUrl,
            int endpointId,
            string imageName,
            string containerName,
            string hostPort
        )
        {
            Console.WriteLine($"Iniciando deploy do container {containerName} usando endpoint ID: {endpointId}...");

            // Renovar token CSRF antes do deploy
            await RenovarTokenCsrf(baseUrl);
            
            string dockerBasePath;
            if (endpointId > 0)
            {
                // Endpoint normal
                dockerBasePath = $"{baseUrl}api/endpoints/{endpointId}/docker";
            }
            else if (endpointId == -2)
            {
                // Endpoint "local"
                dockerBasePath = $"{baseUrl}api/local/docker";
            }
            else if (endpointId == -3)
            {
                // Endpoint "edge"
                dockerBasePath = $"{baseUrl}api/edge/docker";
            }
            else
            {
                // Sem endpoint
                dockerBasePath = $"{baseUrl}api/docker";
            }

            if (debug)
                Console.WriteLine($"Base path para Docker API: {dockerBasePath}");

            // 1. Verificar se o container existe e removê-lo
            await RemoverContainerSeExistir(baseUrl, endpointId, containerName);

            // Renovar token CSRF novamente antes de puxar a imagem
            await RenovarTokenCsrf(baseUrl);
            
            // 2. Puxar a nova imagem
            await PuxarImagem(baseUrl, endpointId, imageName);

            // Renovar token CSRF novamente antes de criar o container
            await RenovarTokenCsrf(baseUrl);
            
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
                // Endpoint normal
                containersUrl =
                    $"{baseUrl}api/endpoints/{endpointId}/docker/containers/json?all=true";
            }
            else if (endpointId == -2)
            {
                // Endpoint "local"
                containersUrl = $"{baseUrl}api/local/docker/containers/json?all=true";
            }
            else if (endpointId == -3)
            {
                // Endpoint "edge"
                containersUrl = $"{baseUrl}api/edge/docker/containers/json?all=true";
            }
            else
            {
                // Sem endpoint
                containersUrl = $"{baseUrl}api/docker/containers/json?all=true";
            }

            if (debug)
                Console.WriteLine($"URL de consulta de containers: {containersUrl}");

            // Usar o método auxiliar para a consulta de containers com token CSRF
            var consultaRequisicao = CriarRequisicaoComCsrf(HttpMethod.Get, containersUrl);
            var response = await client.SendAsync(consultaRequisicao);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(
                    $"Falha ao obter lista de containers. Status: {response.StatusCode}, Resposta: {errorContent}"
                );

                if (debug)
                {
                    Console.WriteLine("Tentando continuar sem verificar containers existentes.");
                    return; // Pula esta etapa e continua com a criação do container
                }
                else
                {
                    throw new Exception(
                        $"Não foi possível obter a lista de containers: {response.StatusCode}"
                    );
                }
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
                else if (endpointId == -2)
                {
                    stopUrl = $"{baseUrl}api/local/docker/containers/{container.Id}/stop";
                }
                else if (endpointId == -3)
                {
                    stopUrl = $"{baseUrl}api/edge/docker/containers/{container.Id}/stop";
                }
                else
                {
                    stopUrl = $"{baseUrl}api/docker/containers/{container.Id}/stop";
                }

                // Usar o método auxiliar para parar o container com token CSRF
                var stopRequisicao = CriarRequisicaoComCsrf(HttpMethod.Post, stopUrl);
                var stopResponse = await client.SendAsync(stopRequisicao);

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
                else if (endpointId == -2)
                {
                    removeUrl = $"{baseUrl}api/local/docker/containers/{container.Id}?force=true";
                }
                else if (endpointId == -3)
                {
                    removeUrl = $"{baseUrl}api/edge/docker/containers/{container.Id}?force=true";
                }
                else
                {
                    removeUrl = $"{baseUrl}api/docker/containers/{container.Id}?force=true";
                }

                // Usar o método auxiliar para remover o container com token CSRF
                var removeRequisicao = CriarRequisicaoComCsrf(HttpMethod.Delete, removeUrl);
                var removeResponse = await client.SendAsync(removeRequisicao);

                if (!removeResponse.IsSuccessStatusCode)
                {
                    var errorContent = await removeResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(
                        $"Falha ao remover o container. Status: {removeResponse.StatusCode}, Resposta: {errorContent}"
                    );

                    // Tentar continuar mesmo com falha
                    Console.WriteLine(
                        "Tentando prosseguir com a criação mesmo sem conseguir remover o container anterior."
                    );
                }
                else
                {
                    Console.WriteLine("Container removido com sucesso.");
                }
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
            else if (endpointId == -2)
            {
                pullUrl = $"{baseUrl}api/local/docker/images/create?fromImage={imageName}";
            }
            else if (endpointId == -3)
            {
                pullUrl = $"{baseUrl}api/edge/docker/images/create?fromImage={imageName}";
            }
            else
            {
                pullUrl = $"{baseUrl}api/docker/images/create?fromImage={imageName}";
            }

            if (debug)
                Console.WriteLine($"URL para pull da imagem: {pullUrl}");

            try
            {
                // Usar o método auxiliar para fazer o pull da imagem com token CSRF
                var pullRequisicao = CriarRequisicaoComCsrf(HttpMethod.Post, pullUrl);
                var response = await client.SendAsync(pullRequisicao);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(
                        $"Falha ao puxar a imagem. Status: {response.StatusCode}, Resposta: {errorContent}"
                    );

                    Console.WriteLine(
                        "Tentando prosseguir mesmo sem conseguir puxar a imagem (ela pode já existir no host)."
                    );
                }
                else
                {
                    Console.WriteLine("Imagem puxada com sucesso.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao puxar imagem: {ex.Message}");

                Console.WriteLine(
                    "Tentando prosseguir mesmo sem conseguir puxar a imagem (ela pode já existir no host)."
                );
            }
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
            else if (endpointId == -2)
            {
                createUrl = $"{baseUrl}api/local/docker/containers/create?name={containerName}";
            }
            else if (endpointId == -3)
            {
                createUrl = $"{baseUrl}api/edge/docker/containers/create?name={containerName}";
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

            // Usar o método auxiliar para criar a requisição com o token CSRF
            var requisicao = CriarRequisicaoComCsrf(HttpMethod.Post, createUrl, content);
            var response = await client.SendAsync(requisicao);

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
            else if (endpointId == -2)
            {
                startUrl = $"{baseUrl}api/local/docker/containers/{containerId}/start";
            }
            else if (endpointId == -3)
            {
                startUrl = $"{baseUrl}api/edge/docker/containers/{containerId}/start";
            }
            else
            {
                startUrl = $"{baseUrl}api/docker/containers/{containerId}/start";
            }

            // Usar o método auxiliar para iniciar o container com token CSRF
            var startRequisicao = CriarRequisicaoComCsrf(HttpMethod.Post, startUrl);
            var startResponse = await client.SendAsync(startRequisicao);

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

        // Método auxiliar para adicionar o token CSRF a uma requisição HTTP
        private static HttpRequestMessage CriarRequisicaoComCsrf(HttpMethod metodo, string url, HttpContent? conteudo = null)
        {
            var requisicao = new HttpRequestMessage(metodo, url);
            
            if (conteudo != null)
            {
                requisicao.Content = conteudo;
            }
            
            // Adicionar o token CSRF se estiver disponível
            if (!string.IsNullOrEmpty(csrfToken))
            {
                // Usar múltiplos headers para garantir compatibilidade
                requisicao.Headers.Add("X-CSRF-Token", csrfToken);
                requisicao.Headers.Add("X-XSRF-Token", csrfToken);
                requisicao.Headers.Add("X-Portainer-CSRF", csrfToken);
                
                // Adicionar também como cookie
                Cookie csrfCookie = new Cookie("portainer.CSRF", csrfToken)
                {
                    Domain = new Uri(url).Host
                };
                cookieContainer.Add(csrfCookie);
                
                if (debug)
                {
                    Console.WriteLine($"Token CSRF adicionado aos cabeçalhos da requisição: {csrfToken}");
                    Console.WriteLine($"Headers na requisição para {url}:");
                    foreach (var header in requisicao.Headers)
                    {
                        Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                    }
                }
            }
            else
            {
                if (debug)
                    Console.WriteLine("AVISO: Requisição sendo feita sem token CSRF");
            }
            
            return requisicao;
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

        private static string? ExtrairTokenCsrfDoHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;
                
            // Várias estratégias para encontrar o token CSRF em diferentes formatos
            
            // 1. Procura no formato: csrfToken: "abc123"
            var match = Regex.Match(
                html,
                @"csrfToken\s*:\s*[""']([^""']+)[""']",
                RegexOptions.IgnoreCase
            );
            if (match.Success)
            {
                Console.WriteLine($"Token CSRF encontrado no HTML (formato csrfToken): {match.Groups[1].Value}");
                return match.Groups[1].Value;
            }
            
            // 2. Procura no formato: <meta name="csrf-token" content="abc123">
            match = Regex.Match(
                html,
                @"<meta\s+name=[""']csrf-token[""']\s+content=[""']([^""']+)[""']",
                RegexOptions.IgnoreCase
            );
            if (match.Success)
            {
                Console.WriteLine($"Token CSRF encontrado no HTML (formato meta tag): {match.Groups[1].Value}");
                return match.Groups[1].Value;
            }
            
            // 3. Procura em variáveis JavaScript
            match = Regex.Match(
                html,
                @"csrf_token\s*=\s*[""']([^""']+)[""']",
                RegexOptions.IgnoreCase
            );
            if (match.Success)
            {
                Console.WriteLine($"Token CSRF encontrado no HTML (formato JS): {match.Groups[1].Value}");
                return match.Groups[1].Value;
            }
            
            // 4. Procura em input hidden
            match = Regex.Match(
                html,
                @"<input\s+[^>]*name=[""'](_csrf|csrf_token|xsrf_token)[""'][^>]*value=[""']([^""']+)[""']",
                RegexOptions.IgnoreCase
            );
            if (match.Success)
            {
                Console.WriteLine($"Token CSRF encontrado no HTML (input hidden): {match.Groups[2].Value}");
                return match.Groups[2].Value;
            }
            
            // 5. Procura em data attributes
            match = Regex.Match(
                html,
                @"data-csrf-token=[""']([^""']+)[""']",
                RegexOptions.IgnoreCase
            );
            if (match.Success)
            {
                Console.WriteLine($"Token CSRF encontrado no HTML (data attribute): {match.Groups[1].Value}");
                return match.Groups[1].Value;
            }
            
            // 6. Procura qualquer string que pareça um token CSRF
            match = Regex.Match(
                html,
                @"csrf[\w\-\.]*[=:][""'\s]*([a-zA-Z0-9\-_\+/=]+)[""'\s]*",
                RegexOptions.IgnoreCase
            );
            if (match.Success)
            {
                Console.WriteLine($"Token CSRF encontrado no HTML (formato genérico): {match.Groups[1].Value}");
                return match.Groups[1].Value;
            }
            
            Console.WriteLine("Nenhum token CSRF encontrado no HTML");
            return null;
        }

        private static async Task<string?> VerificarEdicaoPortainer(string baseUrl)
        {
            try
            {
                Console.WriteLine("Verificando edição do Portainer...");
                var statusUrl = $"{baseUrl}api/status";
                var response = await client.GetAsync(statusUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Resposta de status: {content}");
                    
                    // Verificar se é uma edição específica
                    if (content.Contains("\"Edition\":\"CE\"") || content.Contains("\"Edition\": \"CE\""))
                    {
                        Console.WriteLine("Detectada edição Portainer Community Edition (CE)");
                        return "CE";
                    }
                    else if (content.Contains("\"Edition\":\"BE\"") || content.Contains("\"Edition\": \"BE\""))
                    {
                        Console.WriteLine("Detectada edição Portainer Business Edition (BE)");
                        return "BE";
                    }
                    else if (content.Contains("\"Edition\":\"EE\"") || content.Contains("\"Edition\": \"EE\""))
                    {
                        Console.WriteLine("Detectada edição Portainer Enterprise Edition (EE)");
                        return "EE";
                    }
                }
                
                // Se não conseguiu detectar, tentar pela versão
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content.Contains("\"Version\""))
                    {
                        var versionMatch = Regex.Match(content, @"""Version""\s*:\s*""([^""]+)""");
                        if (versionMatch.Success)
                        {
                            string version = versionMatch.Groups[1].Value;
                            Console.WriteLine($"Versão do Portainer: {version}");
                            
                            // Análise baseada na versão
                            if (version.StartsWith("2."))
                            {
                                Console.WriteLine("Portainer versão 2.x detectada - assumindo Community Edition");
                                return "CE";
                            }
                        }
                    }
                }
                
                Console.WriteLine("Não foi possível determinar a edição do Portainer");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar edição do Portainer: {ex.Message}");
                return null;
            }
        }
    }
}
