using JiraMcp.Models;
using JiraMcp.Services;
using MCPServer.Attributes;

namespace JiraMcp.Tools;

/// <summary>
/// MCP Tools for Jira Server/Instance information (Cloud and Data Center)
/// </summary>
public static class ServerTools
{
    [McpTool("jira_get_server_info", "Gets information about the Jira instance")]
    public static string GetServerInfo()
    {
        return JiraClient.GetStringAsync("serverInfo").GetAwaiter().GetResult();
    }

    [McpTool("jira_get_fields", "Gets all available fields in the Jira instance")]
    public static string GetFields()
    {
        return JiraClient.GetStringAsync("field").GetAwaiter().GetResult();
    }

    [McpTool("jira_get_custom_fields", "Gets all custom fields in the Jira instance")]
    public static string GetCustomFields()
    {
        var rawJson = JiraClient.GetStringAsync("field").GetAwaiter().GetResult();
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
            var customFields = doc.RootElement.EnumerateArray()
                .Where(e => e.TryGetProperty("custom", out var isCustom) && isCustom.ValueKind == System.Text.Json.JsonValueKind.True)
                .Select(e => e.Clone())
                .ToList();
            return JiraClient.ToJson(customFields);
        }
        catch { }
        return "[]";
    }

    [McpTool("jira_validate_jql", "Validates a JQL query for syntax errors")]
    public static string ValidateJql(
        [McpParameter("JQL query to validate")] string jql)
    {
        var request = new { queries = new[] { jql } };
        return JiraClient.PostStringAsync("jql/parse", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_get_autocomplete_suggestions", "Gets JQL autocomplete suggestions")]
    public static string GetAutocompleteSuggestions(
        [McpParameter("Field name to get suggestions for")] string fieldName,
        [McpParameter("Value prefix to filter suggestions", false)] string? fieldValue = null)
    {
        var endpoint = $"jql/autocompletedata/suggestions?fieldName={Uri.EscapeDataString(fieldName)}";
        
        if (!string.IsNullOrEmpty(fieldValue))
        {
            endpoint += $"&fieldValue={Uri.EscapeDataString(fieldValue)}";
        }

        return JiraClient.GetStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_check_connection", "Checks if the Jira connection is working")]
    public static string CheckConnection()
    {
        try
        {
            var userJson = JiraClient.GetStringAsync("myself").GetAwaiter().GetResult();
            var displayName = "Unknown";
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(userJson);
                if (doc.RootElement.TryGetProperty("displayName", out var disp)) displayName = disp.GetString() ?? "Unknown";
                else if (doc.RootElement.TryGetProperty("name", out var nm)) displayName = nm.GetString() ?? "Unknown";
            }
            catch { }
            
            return JiraClient.ToJson(new
            {
                connected = true,
                baseUrl = JiraClient.BaseUrl,
                isCloud = JiraClient.IsCloud,
                deploymentType = JiraClient.IsCloud ? "Cloud" : "Data Center / Server",
                authenticatedUser = displayName
            });
        }
        catch (Exception ex)
        {
            return JiraClient.ToJson(new
            {
                connected = false,
                baseUrl = JiraClient.BaseUrl,
                isCloud = JiraClient.IsCloud,
                error = ex.Message
            });
        }
    }

    [McpTool("jira_get_configuration", "Gets the current MCP server configuration (without sensitive data)")]
    public static string GetConfiguration()
    {
        var hasEmail = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JIRA_EMAIL"));
        var hasApiToken = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JIRA_API_TOKEN"));
        var hasPat = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JIRA_PAT"));
        var hasUsername = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JIRA_USERNAME"));
        var hasPassword = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JIRA_PASSWORD"));

        return JiraClient.ToJson(new
        {
            baseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL") ?? "(not set)",
            isCloud = JiraClient.IsCloud,
            apiVersion = Environment.GetEnvironmentVariable("JIRA_API_VERSION") ?? "auto",
            authentication = new
            {
                cloudAuth = new { emailConfigured = hasEmail, apiTokenConfigured = hasApiToken },
                dataCenterAuth = new { patConfigured = hasPat, usernameConfigured = hasUsername, passwordConfigured = hasPassword }
            }
        });
    }
}


