using JiraMcp.Models;
using JiraMcp.Services;
using MCPServer.Attributes;

namespace JiraMcp.Tools;

/// <summary>
/// MCP Tools for Jira Project operations (Cloud and Data Center)
/// </summary>
public static class ProjectTools
{
    [McpTool("jira_list_projects", "Lists all accessible Jira projects")]
    public static async Task<string> ListProjects(
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("Filter by project keys (comma-separated)", false)] string? keys = null,
        [McpParameter("Comma-separated fields to expand (e.g., description,lead,issueTypes)", false)] string? expand = null)
    {
        var endpoint = $"project?startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(keys))
        {
            endpoint += $"&keys={keys}";
        }
        
        if (!string.IsNullOrEmpty(expand))
        {
            endpoint += $"&expand={expand}";
        }

        var result = await JiraClient.GetAsync<List<JiraProject>>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_project", "Gets detailed information about a specific project")]
    public static async Task<string> GetProject(
        [McpParameter("Project key (e.g., PROJ) or project ID")] string projectKey,
        [McpParameter("Comma-separated fields to expand (e.g., description,lead,issueTypes,components,versions)", false)] string? expand = null)
    {
        var endpoint = $"project/{projectKey}";
        
        if (!string.IsNullOrEmpty(expand))
        {
            endpoint += $"?expand={expand}";
        }

        var result = await JiraClient.GetAsync<JiraProject>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_create_project", "Creates a new Jira project")]
    public static async Task<string> CreateProject(
        [McpParameter("Project key (uppercase letters, e.g., PROJ)")] string key,
        [McpParameter("Project name")] string name,
        [McpParameter("Project type key (e.g., software, business, service_desk)")] string projectTypeKey,
        [McpParameter("Project description", false)] string? description = null,
        [McpParameter("Lead account ID (Cloud) or username (Data Center)", false)] string? lead = null,
        [McpParameter("Project category ID", false)] string? categoryId = null,
        [McpParameter("Default assignee type (PROJECT_LEAD, UNASSIGNED)", false)] string? assigneeType = null)
    {
        var request = new CreateProjectRequest(
            Key: key,
            Name: name,
            ProjectTypeKey: projectTypeKey,
            Description: description,
            LeadAccountId: JiraClient.IsCloud ? lead : null,
            Lead: !JiraClient.IsCloud ? lead : null,
            CategoryId: categoryId,
            AssigneeType: assigneeType
        );

        var result = await JiraClient.PostAsync<JiraProject>("project", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_update_project", "Updates an existing Jira project")]
    public static async Task<string> UpdateProject(
        [McpParameter("Project key or ID")] string projectKey,
        [McpParameter("New project name", false)] string? name = null,
        [McpParameter("New project description", false)] string? description = null,
        [McpParameter("New lead account ID (Cloud) or username (Data Center)", false)] string? lead = null,
        [McpParameter("New project category ID", false)] string? categoryId = null,
        [McpParameter("New assignee type (PROJECT_LEAD, UNASSIGNED)", false)] string? assigneeType = null)
    {
        var request = new Dictionary<string, object?>();
        
        if (!string.IsNullOrEmpty(name)) request["name"] = name;
        if (!string.IsNullOrEmpty(description)) request["description"] = description;
        if (!string.IsNullOrEmpty(lead))
        {
            if (JiraClient.IsCloud)
                request["leadAccountId"] = lead;
            else
                request["lead"] = lead;
        }
        if (!string.IsNullOrEmpty(categoryId)) request["categoryId"] = categoryId;
        if (!string.IsNullOrEmpty(assigneeType)) request["assigneeType"] = assigneeType;

        var result = await JiraClient.PutAsync<JiraProject>($"project/{projectKey}", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_delete_project", "Deletes a Jira project (moves to trash on Cloud)")]
    public static async Task<string> DeleteProject(
        [McpParameter("Project key or ID")] string projectKey)
    {
        await JiraClient.DeleteAsync($"project/{projectKey}");
        return $"Project {projectKey} deleted successfully";
    }

    [McpTool("jira_get_project_components", "Gets all components for a project")]
    public static async Task<string> GetProjectComponents(
        [McpParameter("Project key or ID")] string projectKey)
    {
        var result = await JiraClient.GetAsync<List<JiraComponent>>($"project/{projectKey}/components");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_create_component", "Creates a new component in a project")]
    public static async Task<string> CreateComponent(
        [McpParameter("Project key")] string projectKey,
        [McpParameter("Component name")] string name,
        [McpParameter("Component description", false)] string? description = null,
        [McpParameter("Lead account ID (Cloud) or username (Data Center)", false)] string? lead = null,
        [McpParameter("Default assignee type (PROJECT_DEFAULT, COMPONENT_LEAD, PROJECT_LEAD, UNASSIGNED)", false)] string? assigneeType = null)
    {
        var request = new Dictionary<string, object?>
        {
            ["project"] = projectKey,
            ["name"] = name
        };
        
        if (!string.IsNullOrEmpty(description)) request["description"] = description;
        if (!string.IsNullOrEmpty(lead))
        {
            if (JiraClient.IsCloud)
                request["leadAccountId"] = lead;
            else
                request["leadUserName"] = lead;
        }
        if (!string.IsNullOrEmpty(assigneeType)) request["assigneeType"] = assigneeType;

        var result = await JiraClient.PostAsync<JiraComponent>("component", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_delete_component", "Deletes a component")]
    public static async Task<string> DeleteComponent(
        [McpParameter("Component ID")] string componentId,
        [McpParameter("ID of component to move issues to", false)] string? moveIssuesTo = null)
    {
        var endpoint = $"component/{componentId}";
        if (!string.IsNullOrEmpty(moveIssuesTo))
        {
            endpoint += $"?moveIssuesTo={moveIssuesTo}";
        }

        await JiraClient.DeleteAsync(endpoint);
        return $"Component {componentId} deleted";
    }

    [McpTool("jira_get_project_versions", "Gets all versions for a project")]
    public static async Task<string> GetProjectVersions(
        [McpParameter("Project key or ID")] string projectKey)
    {
        var result = await JiraClient.GetAsync<List<JiraVersion>>($"project/{projectKey}/versions");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_create_version", "Creates a new version in a project")]
    public static async Task<string> CreateVersion(
        [McpParameter("Project key or ID")] string projectKey,
        [McpParameter("Version name")] string name,
        [McpParameter("Version description", false)] string? description = null,
        [McpParameter("Release date (YYYY-MM-DD)", false)] string? releaseDate = null,
        [McpParameter("Start date (YYYY-MM-DD)", false)] string? startDate = null,
        [McpParameter("Whether the version is released", false)] bool released = false,
        [McpParameter("Whether the version is archived", false)] bool archived = false)
    {
        var request = new Dictionary<string, object?>
        {
            ["project"] = projectKey,
            ["name"] = name,
            ["released"] = released,
            ["archived"] = archived
        };
        
        if (!string.IsNullOrEmpty(description)) request["description"] = description;
        if (!string.IsNullOrEmpty(releaseDate)) request["releaseDate"] = releaseDate;
        if (!string.IsNullOrEmpty(startDate)) request["startDate"] = startDate;

        var result = await JiraClient.PostAsync<JiraVersion>("version", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_update_version", "Updates a version")]
    public static async Task<string> UpdateVersion(
        [McpParameter("Version ID")] string versionId,
        [McpParameter("New name", false)] string? name = null,
        [McpParameter("New description", false)] string? description = null,
        [McpParameter("Release date (YYYY-MM-DD)", false)] string? releaseDate = null,
        [McpParameter("Whether the version is released", false)] bool? released = null,
        [McpParameter("Whether the version is archived", false)] bool? archived = null)
    {
        var request = new Dictionary<string, object?>();
        
        if (!string.IsNullOrEmpty(name)) request["name"] = name;
        if (!string.IsNullOrEmpty(description)) request["description"] = description;
        if (!string.IsNullOrEmpty(releaseDate)) request["releaseDate"] = releaseDate;
        if (released.HasValue) request["released"] = released.Value;
        if (archived.HasValue) request["archived"] = archived.Value;

        var result = await JiraClient.PutAsync<JiraVersion>($"version/{versionId}", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_release_version", "Releases a version")]
    public static async Task<string> ReleaseVersion(
        [McpParameter("Version ID")] string versionId,
        [McpParameter("Release date (YYYY-MM-DD). Defaults to today.", false)] string? releaseDate = null)
    {
        var request = new Dictionary<string, object?>
        {
            ["released"] = true,
            ["releaseDate"] = releaseDate ?? DateTime.Today.ToString("yyyy-MM-dd")
        };

        var result = await JiraClient.PutAsync<JiraVersion>($"version/{versionId}", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_delete_version", "Deletes a version")]
    public static async Task<string> DeleteVersion(
        [McpParameter("Version ID")] string versionId,
        [McpParameter("Version ID to move fix issues to", false)] string? moveFixIssuesTo = null,
        [McpParameter("Version ID to move affected issues to", false)] string? moveAffectedIssuesTo = null)
    {
        var endpoint = $"version/{versionId}";
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(moveFixIssuesTo))
            queryParams.Add($"moveFixIssuesTo={moveFixIssuesTo}");
        if (!string.IsNullOrEmpty(moveAffectedIssuesTo))
            queryParams.Add($"moveAffectedIssuesTo={moveAffectedIssuesTo}");
        
        if (queryParams.Count > 0)
            endpoint += "?" + string.Join("&", queryParams);

        await JiraClient.DeleteAsync(endpoint);
        return $"Version {versionId} deleted";
    }

    [McpTool("jira_get_issue_types", "Gets all issue types available in the system")]
    public static async Task<string> GetIssueTypes()
    {
        var result = await JiraClient.GetAsync<List<IssueType>>("issuetype");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_project_issue_types", "Gets issue types available for a specific project")]
    public static async Task<string> GetProjectIssueTypes(
        [McpParameter("Project key or ID")] string projectKey)
    {
        var result = await JiraClient.GetAsync<JiraProject>($"project/{projectKey}?expand=issueTypes");
        return JiraClient.ToJson(result?.IssueTypes);
    }

    [McpTool("jira_get_priorities", "Gets all available priorities")]
    public static async Task<string> GetPriorities()
    {
        var result = await JiraClient.GetAsync<List<JiraPriority>>("priority");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_statuses", "Gets all statuses available in the system")]
    public static async Task<string> GetStatuses()
    {
        var result = await JiraClient.GetAsync<List<JiraStatus>>("status");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_resolutions", "Gets all available resolutions")]
    public static async Task<string> GetResolutions()
    {
        var result = await JiraClient.GetAsync<List<JiraResolution>>("resolution");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_project_roles", "Gets all roles for a project")]
    public static async Task<string> GetProjectRoles(
        [McpParameter("Project key or ID")] string projectKey)
    {
        var result = await JiraClient.GetAsync<Dictionary<string, string>>($"project/{projectKey}/role");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_project_role", "Gets users/groups assigned to a specific role in a project")]
    public static async Task<string> GetProjectRole(
        [McpParameter("Project key or ID")] string projectKey,
        [McpParameter("Role ID")] string roleId)
    {
        var result = await JiraClient.GetAsync<object>($"project/{projectKey}/role/{roleId}");
        return JiraClient.ToJson(result);
    }
}
