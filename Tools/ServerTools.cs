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
        var result = JiraClient.GetAsync<ServerInfo>("serverInfo").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_fields", "Gets all available fields in the Jira instance")]
    public static string GetFields()
    {
        var result = JiraClient.GetAsync<List<JiraField>>("field").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_custom_fields", "Gets all custom fields in the Jira instance")]
    public static string GetCustomFields()
    {
        var fields = JiraClient.GetAsync<List<JiraField>>("field").GetAwaiter().GetResult();
        var customFields = fields?.Where(f => f.Custom == true).ToList();
        return JiraClient.ToJson(customFields);
    }

    [McpTool("jira_validate_jql", "Validates a JQL query for syntax errors")]
    public static string ValidateJql(
        [McpParameter("JQL query to validate")] string jql)
    {
        var request = new { queries = new[] { jql } };
        var result = JiraClient.PostAsync<object>("jql/parse", request).GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
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

        var result = JiraClient.GetAsync<object>(endpoint).GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_check_connection", "Checks if the Jira connection is working")]
    public static string CheckConnection()
    {
        try
        {
            var user = JiraClient.GetAsync<JiraUser>("myself").GetAwaiter().GetResult();
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
