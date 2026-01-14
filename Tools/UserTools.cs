using JiraMcp.Models;
using JiraMcp.Services;
using MCPServer.Attributes;

namespace JiraMcp.Tools;

/// <summary>
/// MCP Tools for Jira User operations (Cloud and Data Center)
/// </summary>
public static class UserTools
{
    [McpTool("jira_get_current_user", "Gets information about the currently authenticated user")]
    public static async Task<string> GetCurrentUser()
    {
        var result = await JiraClient.GetAsync<JiraUser>("myself");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_user", "Gets information about a specific user")]
    public static async Task<string> GetUser(
        [McpParameter("Account ID (Cloud) or username/key (Data Center)")] string userIdentifier)
    {
        string endpoint;
        
        if (JiraClient.IsCloud)
        {
            endpoint = $"user?accountId={Uri.EscapeDataString(userIdentifier)}";
        }
        else
        {
            // Data Center supports both username and key
            endpoint = $"user?username={Uri.EscapeDataString(userIdentifier)}";
        }

        var result = await JiraClient.GetAsync<JiraUser>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_search_users", "Searches for users by name, email, or username")]
    public static async Task<string> SearchUsers(
        [McpParameter("Search query (name, email, or username)")] string query,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50)
    {
        string endpoint;
        
        if (JiraClient.IsCloud)
        {
            // Cloud uses the user/search endpoint with query parameter
            endpoint = $"user/search?query={Uri.EscapeDataString(query)}&startAt={startAt}&maxResults={maxResults}";
        }
        else
        {
            // Data Center uses user/search with username parameter
            endpoint = $"user/search?username={Uri.EscapeDataString(query)}&startAt={startAt}&maxResults={maxResults}";
        }

        var result = await JiraClient.GetAsync<List<JiraUser>>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_find_users_assignable_to_projects", "Finds users that can be assigned to issues in specified projects")]
    public static async Task<string> FindAssignableUsers(
        [McpParameter("Project key(s), comma-separated for multiple")] string projectKeys,
        [McpParameter("Username or name to filter by", false)] string? query = null,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50)
    {
        var endpoint = $"user/assignable/multiProjectSearch?projectKeys={Uri.EscapeDataString(projectKeys)}&startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(query))
        {
            endpoint += $"&query={Uri.EscapeDataString(query)}";
        }

        var result = await JiraClient.GetAsync<List<JiraUser>>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_find_users_assignable_to_issue", "Finds users that can be assigned to a specific issue")]
    public static async Task<string> FindUsersAssignableToIssue(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Username or name to filter by", false)] string? query = null,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50)
    {
        var endpoint = $"user/assignable/search?issueKey={Uri.EscapeDataString(issueKey)}&startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(query))
        {
            endpoint += $"&query={Uri.EscapeDataString(query)}";
        }

        var result = await JiraClient.GetAsync<List<JiraUser>>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_users_from_group", "Gets all users in a specific group")]
    public static async Task<string> GetUsersFromGroup(
        [McpParameter("Group name")] string groupName,
        [McpParameter("Whether to include inactive users", false)] bool includeInactiveUsers = false,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50)
    {
        var endpoint = $"group/member?groupname={Uri.EscapeDataString(groupName)}&includeInactiveUsers={includeInactiveUsers}&startAt={startAt}&maxResults={maxResults}";
        var result = await JiraClient.GetAsync<PagedResult<JiraUser>>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_groups", "Gets all groups in Jira")]
    public static async Task<string> GetGroups(
        [McpParameter("Filter groups by name containing this string", false)] string? query = null,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50)
    {
        var endpoint = $"groups/picker?maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(query))
        {
            endpoint += $"&query={Uri.EscapeDataString(query)}";
        }

        var result = await JiraClient.GetAsync<object>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_add_user_to_group", "Adds a user to a group (requires admin permissions)")]
    public static async Task<string> AddUserToGroup(
        [McpParameter("Group name")] string groupName,
        [McpParameter("Account ID (Cloud) or username (Data Center)")] string userIdentifier)
    {
        object request;
        
        if (JiraClient.IsCloud)
        {
            request = new { accountId = userIdentifier };
        }
        else
        {
            request = new { name = userIdentifier };
        }

        await JiraClient.PostAsync<object>($"group/user?groupname={Uri.EscapeDataString(groupName)}", request);
        return $"User {userIdentifier} added to group {groupName}";
    }

    [McpTool("jira_remove_user_from_group", "Removes a user from a group (requires admin permissions)")]
    public static async Task<string> RemoveUserFromGroup(
        [McpParameter("Group name")] string groupName,
        [McpParameter("Account ID (Cloud) or username (Data Center)")] string userIdentifier)
    {
        string endpoint;
        
        if (JiraClient.IsCloud)
        {
            endpoint = $"group/user?groupname={Uri.EscapeDataString(groupName)}&accountId={Uri.EscapeDataString(userIdentifier)}";
        }
        else
        {
            endpoint = $"group/user?groupname={Uri.EscapeDataString(groupName)}&username={Uri.EscapeDataString(userIdentifier)}";
        }

        await JiraClient.DeleteAsync(endpoint);
        return $"User {userIdentifier} removed from group {groupName}";
    }

    [McpTool("jira_find_users_with_permissions", "Finds users with specific permissions on a project or issue")]
    public static async Task<string> FindUsersWithPermissions(
        [McpParameter("Comma-separated list of permission names (e.g., BROWSE_PROJECTS,EDIT_ISSUES)")] string permissions,
        [McpParameter("Project key to check permissions for", false)] string? projectKey = null,
        [McpParameter("Issue key to check permissions for", false)] string? issueKey = null,
        [McpParameter("Username or name to filter by", false)] string? query = null,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50)
    {
        var endpoint = $"user/permission/search?permissions={Uri.EscapeDataString(permissions)}&startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(projectKey))
        {
            endpoint += $"&projectKey={Uri.EscapeDataString(projectKey)}";
        }
        
        if (!string.IsNullOrEmpty(issueKey))
        {
            endpoint += $"&issueKey={Uri.EscapeDataString(issueKey)}";
        }
        
        if (!string.IsNullOrEmpty(query))
        {
            endpoint += $"&query={Uri.EscapeDataString(query)}";
        }

        var result = await JiraClient.GetAsync<List<JiraUser>>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_user_permissions", "Gets permissions the current user has globally or on a specific project/issue")]
    public static async Task<string> GetMyPermissions(
        [McpParameter("Project key to check permissions for", false)] string? projectKey = null,
        [McpParameter("Issue key to check permissions for", false)] string? issueKey = null)
    {
        var endpoint = "mypermissions";
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(projectKey))
        {
            queryParams.Add($"projectKey={Uri.EscapeDataString(projectKey)}");
        }
        
        if (!string.IsNullOrEmpty(issueKey))
        {
            queryParams.Add($"issueKey={Uri.EscapeDataString(issueKey)}");
        }
        
        if (queryParams.Count > 0)
        {
            endpoint += "?" + string.Join("&", queryParams);
        }

        var result = await JiraClient.GetAsync<object>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_bulk_get_users", "Gets multiple users by their account IDs (Cloud) or usernames (Data Center)")]
    public static async Task<string> BulkGetUsers(
        [McpParameter("Comma-separated list of account IDs (Cloud) or usernames (Data Center)")] string userIdentifiers,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50)
    {
        var identifiers = userIdentifiers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        if (JiraClient.IsCloud)
        {
            var accountIds = string.Join("&accountId=", identifiers.Select(Uri.EscapeDataString));
            var endpoint = $"user/bulk?accountId={accountIds}&startAt={startAt}&maxResults={maxResults}";
            var result = await JiraClient.GetAsync<PagedResult<JiraUser>>(endpoint);
            return JiraClient.ToJson(result);
        }
        else
        {
            // Data Center doesn't have bulk endpoint, fetch one by one
            var users = new List<JiraUser>();
            foreach (var username in identifiers)
            {
                var user = await JiraClient.GetAsync<JiraUser>($"user?username={Uri.EscapeDataString(username)}");
                if (user != null)
                {
                    users.Add(user);
                }
            }
            return JiraClient.ToJson(users);
        }
    }
}
