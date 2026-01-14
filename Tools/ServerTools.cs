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
    public static async Task<string> GetServerInfo()
    {
        var result = await JiraClient.GetAsync<ServerInfo>("serverInfo");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_fields", "Gets all available fields in the Jira instance")]
    public static async Task<string> GetFields()
    {
        var result = await JiraClient.GetAsync<List<JiraField>>("field");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_custom_fields", "Gets all custom fields in the Jira instance")]
    public static async Task<string> GetCustomFields()
    {
        var fields = await JiraClient.GetAsync<List<JiraField>>("field");
        var customFields = fields?.Where(f => f.Custom == true).ToList();
        return JiraClient.ToJson(customFields);
    }

    [McpTool("jira_validate_jql", "Validates a JQL query for syntax errors")]
    public static async Task<string> ValidateJql(
        [McpParameter("JQL query to validate")] string jql)
    {
        var request = new { queries = new[] { jql } };
        var result = await JiraClient.PostAsync<object>("jql/parse", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_autocomplete_suggestions", "Gets JQL autocomplete suggestions")]
    public static async Task<string> GetAutocompleteSuggestions(
        [McpParameter("Field name to get suggestions for")] string fieldName,
        [McpParameter("Value prefix to filter suggestions", false)] string? fieldValue = null)
    {
        var endpoint = $"jql/autocompletedata/suggestions?fieldName={Uri.EscapeDataString(fieldName)}";
        
        if (!string.IsNullOrEmpty(fieldValue))
        {
            endpoint += $"&fieldValue={Uri.EscapeDataString(fieldValue)}";
        }

        var result = await JiraClient.GetAsync<object>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_check_connection", "Checks if the Jira connection is working")]
    public static async Task<string> CheckConnection()
    {
        try
        {
            var user = await JiraClient.GetAsync<JiraUser>("myself");
            return JiraClient.ToJson(new
            {
                connected = true,
                baseUrl = JiraClient.BaseUrl,
                isCloud = JiraClient.IsCloud,
                deploymentType = JiraClient.IsCloud ? "Cloud" : "Data Center / Server",
                authenticatedUser = user?.DisplayName ?? user?.Name ?? "Unknown"
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
