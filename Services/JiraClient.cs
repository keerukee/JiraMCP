using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraMcp.Services;

/// <summary>
/// HTTP client for communicating with Jira REST API (both Cloud and Data Center)
/// </summary>
public static class JiraClient
{
    private static readonly HttpClient _httpClient = new();
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Gets the configured Jira base URL from environment variables
    /// </summary>
    public static string BaseUrl => Environment.GetEnvironmentVariable("JIRA_BASE_URL") 
        ?? throw new InvalidOperationException("JIRA_BASE_URL environment variable is not set");

    /// <summary>
    /// Determines if running against Jira Cloud (vs Data Center).
    /// Set JIRA_IS_CLOUD environment variable to "true" for Cloud, "false" for Data Center.
    /// </summary>
    public static bool IsCloud => string.Equals(
        Environment.GetEnvironmentVariable("JIRA_IS_CLOUD"), 
        "true", 
        StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets authentication headers based on the Jira deployment type
    /// </summary>
    private static AuthenticationHeaderValue GetAuthHeader()
    {
        if (IsCloud)
        {
            // Jira Cloud uses email + API token
            var email = Environment.GetEnvironmentVariable("JIRA_EMAIL")
                ?? throw new InvalidOperationException("JIRA_EMAIL environment variable is not set for Cloud authentication");
            var apiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN")
                ?? throw new InvalidOperationException("JIRA_API_TOKEN environment variable is not set");
            
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{email}:{apiToken}"));
            return new AuthenticationHeaderValue("Basic", credentials);
        }
        else
        {
            // Jira Data Center can use PAT or username/password
            var pat = Environment.GetEnvironmentVariable("JIRA_PAT");
            if (!string.IsNullOrEmpty(pat))
            {
                return new AuthenticationHeaderValue("Bearer", pat);
            }

            var username = Environment.GetEnvironmentVariable("JIRA_USERNAME")
                ?? throw new InvalidOperationException("JIRA_USERNAME or JIRA_PAT environment variable is not set for Data Center authentication");
            var password = Environment.GetEnvironmentVariable("JIRA_PASSWORD")
                ?? throw new InvalidOperationException("JIRA_PASSWORD environment variable is not set");
            
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            return new AuthenticationHeaderValue("Basic", credentials);
        }
    }

    /// <summary>
    /// Constructs the full API URL based on deployment type
    /// </summary>
    private static string GetApiUrl(string endpoint)
    {
        var baseUrl = BaseUrl.TrimEnd('/');
        
        // Cloud and Data Center both use /rest/api/3 for most operations
        // Data Center also supports /rest/api/2 for backward compatibility
        var apiVersion = IsCloud ? "3" : 
            (Environment.GetEnvironmentVariable("JIRA_API_VERSION") ?? "2");
        
        if (endpoint.StartsWith("/rest/"))
        {
            return $"{baseUrl}{endpoint}";
        }
        
        return $"{baseUrl}/rest/api/{apiVersion}/{endpoint.TrimStart('/')}";
    }

    /// <summary>
    /// Constructs the Agile API URL
    /// </summary>
    private static string GetAgileApiUrl(string endpoint)
    {
        var baseUrl = BaseUrl.TrimEnd('/');
        return $"{baseUrl}/rest/agile/1.0/{endpoint.TrimStart('/')}";
    }

    /// <summary>
    /// Sends a GET request to the Jira API
    /// </summary>
    public static async Task<T?> GetAsync<T>(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, GetApiUrl(endpoint));
        request.Headers.Authorization = GetAuthHeader();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Jira API error ({response.StatusCode}): {content}");
        }

        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    /// <summary>
    /// Sends a GET request to the Jira Agile API
    /// </summary>
    public static async Task<T?> GetAgileAsync<T>(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, GetAgileApiUrl(endpoint));
        request.Headers.Authorization = GetAuthHeader();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Jira Agile API error ({response.StatusCode}): {content}");
        }

        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    /// <summary>
    /// Sends a POST request to the Jira API
    /// </summary>
    public static async Task<T?> PostAsync<T>(string endpoint, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, GetApiUrl(endpoint));
        request.Headers.Authorization = GetAuthHeader();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Jira API error ({response.StatusCode}): {content}");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    /// <summary>
    /// Sends a POST request to the Jira Agile API
    /// </summary>
    public static async Task<T?> PostAgileAsync<T>(string endpoint, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, GetAgileApiUrl(endpoint));
        request.Headers.Authorization = GetAuthHeader();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Jira Agile API error ({response.StatusCode}): {content}");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    /// <summary>
    /// Sends a PUT request to the Jira API
    /// </summary>
    public static async Task<T?> PutAsync<T>(string endpoint, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, GetApiUrl(endpoint));
        request.Headers.Authorization = GetAuthHeader();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Jira API error ({response.StatusCode}): {content}");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    /// <summary>
    /// Sends a DELETE request to the Jira API
    /// </summary>
    public static async Task DeleteAsync(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, GetApiUrl(endpoint));
        request.Headers.Authorization = GetAuthHeader();

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Jira API error ({response.StatusCode}): {content}");
        }
    }

    /// <summary>
    /// Sends a DELETE request to the Jira Agile API
    /// </summary>
    public static async Task DeleteAgileAsync(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, GetAgileApiUrl(endpoint));
        request.Headers.Authorization = GetAuthHeader();

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Jira Agile API error ({response.StatusCode}): {content}");
        }
    }

    /// <summary>
    /// Uploads a file attachment to Jira
    /// </summary>
    public static async Task<T?> UploadFileAsync<T>(string endpoint, byte[] fileContent, string fileName)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, GetApiUrl(endpoint));
        request.Headers.Authorization = GetAuthHeader();
        request.Headers.Add("X-Atlassian-Token", "no-check");

        var content = new MultipartFormDataContent();
        var fileContentPart = new ByteArrayContent(fileContent);
        fileContentPart.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContentPart, "file", fileName);
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Jira API error ({response.StatusCode}): {responseContent}");
        }

        return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
    }

    /// <summary>
    /// Serializes an object to JSON string
    /// </summary>
    public static string ToJson(object obj) => JsonSerializer.Serialize(obj, _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to an object
    /// </summary>
    public static T? FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, _jsonOptions);

    /// <summary>
    /// Converts a plain text string to Atlassian Document Format (ADF).
    /// ADF is required by Jira Cloud V3 API for rich text fields like description, environment, and comments.
    /// Splits text on newlines to create separate paragraph nodes.
    /// </summary>
    public static object ToAdfDocument(string text)
    {
        var paragraphs = text.Split('\n')
            .Select(line => new Dictionary<string, object>
            {
                ["type"] = "paragraph",
                ["content"] = string.IsNullOrEmpty(line)
                    ? Array.Empty<object>()
                    : new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["type"] = "text",
                            ["text"] = line
                        }
                    }
            })
            .ToArray();

        return new Dictionary<string, object>
        {
            ["type"] = "doc",
            ["version"] = 1,
            ["content"] = paragraphs
        };
    }

    /// <summary>
    /// Conditionally formats text for the Jira API.
    /// Returns ADF object for Cloud (V3), plain string for Data Center (V2).
    /// Returns null if input is null or empty.
    /// </summary>
    public static object? FormatTextForApi(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        return IsCloud ? ToAdfDocument(text) : text;
    }
}
