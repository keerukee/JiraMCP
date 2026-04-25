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
    public static string ListProjects(
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

        var result = JiraClient.GetAsync<List<JiraProject>>(endpoint).GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_project", "Gets detailed information about a specific project")]
    public static string GetProject(
        [McpParameter("Project key (e.g., PROJ) or project ID")] string projectKey,
        [McpParameter("Comma-separated fields to expand (e.g., description,lead,issueTypes,components,versions)", false)] string? expand = null)
    {
        var endpoint = $"project/{projectKey}";
        
        if (!string.IsNullOrEmpty(expand))
        {
            endpoint += $"?expand={expand}";
        }

        return JiraClient.GetStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_create_project", "Creates a new Jira project")]
    public static string CreateProject(
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

        return JiraClient.PostStringAsync("project", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_update_project", "Updates an existing Jira project")]
    public static string UpdateProject(
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

        return JiraClient.PutStringAsync($"project/{projectKey}", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_delete_project", "Deletes a Jira project (moves to trash on Cloud)")]
    public static string DeleteProject(
        [McpParameter("Project key or ID")] string projectKey)
    {
        JiraClient.DeleteAsync($"project/{projectKey}").GetAwaiter().GetResult();
        return $"Project {projectKey} deleted successfully";
    }

    [McpTool("jira_get_project_components", "Gets all components for a project")]
    public static string GetProjectComponents(
        [McpParameter("Project key or ID")] string projectKey)
    {
        var result = JiraClient.GetAsync<List<JiraComponent>>($"project/{projectKey}/components").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_create_component", "Creates a new component in a project")]
    public static string CreateComponent(
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

        return JiraClient.PostStringAsync("component", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_delete_component", "Deletes a component")]
    public static string DeleteComponent(
        [McpParameter("Component ID")] string componentId,
        [McpParameter("ID of component to move issues to", false)] string? moveIssuesTo = null)
    {
        var endpoint = $"component/{componentId}";
        if (!string.IsNullOrEmpty(moveIssuesTo))
        {
            endpoint += $"?moveIssuesTo={moveIssuesTo}";
        }

        JiraClient.DeleteAsync(endpoint).GetAwaiter().GetResult();
        return $"Component {componentId} deleted";
    }

    [McpTool("jira_get_project_versions", "Gets all versions for a project")]
    public static string GetProjectVersions(
        [McpParameter("Project key or ID")] string projectKey)
    {
        var result = JiraClient.GetAsync<List<JiraVersion>>($"project/{projectKey}/versions").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_create_version", "Creates a new version in a project")]
    public static string CreateVersion(
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

        return JiraClient.PostStringAsync("version", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_update_version", "Updates a version")]
    public static string UpdateVersion(
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

        return JiraClient.PutStringAsync($"version/{versionId}", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_release_version", "Releases a version")]
    public static string ReleaseVersion(
        [McpParameter("Version ID")] string versionId,
        [McpParameter("Release date (YYYY-MM-DD). Defaults to today.", false)] string? releaseDate = null)
    {
        var request = new Dictionary<string, object?>
        {
            ["released"] = true,
            ["releaseDate"] = releaseDate ?? DateTime.Today.ToString("yyyy-MM-dd")
        };

        return JiraClient.PutStringAsync($"version/{versionId}", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_delete_version", "Deletes a version")]
    public static string DeleteVersion(
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

        JiraClient.DeleteAsync(endpoint).GetAwaiter().GetResult();
        return $"Version {versionId} deleted";
    }

    [McpTool("jira_get_issue_types", "Gets all issue types available in the system")]
    public static string GetIssueTypes()
    {
        var result = JiraClient.GetAsync<List<IssueType>>("issuetype").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_project_issue_types", "Gets issue types available for a specific project")]
    public static string GetProjectIssueTypes(
        [McpParameter("Project key or ID")] string projectKey)
    {
        var result = JiraClient.GetAsync<JiraProject>($"project/{projectKey}?expand=issueTypes").GetAwaiter().GetResult();
        return JiraClient.ToJson(result?.IssueTypes);
    }

    [McpTool("jira_get_priorities", "Gets all available priorities")]
    public static string GetPriorities()
    {
        return JiraClient.GetStringAsync("priority").GetAwaiter().GetResult();
    }

    [McpTool("jira_get_statuses", "Gets all statuses available in the system")]
    public static string GetStatuses()
    {
        var result = JiraClient.GetAsync<List<JiraStatus>>("status").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_resolutions", "Gets all available resolutions")]
    public static string GetResolutions()
    {
        var result = JiraClient.GetAsync<List<JiraResolution>>("resolution").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_project_roles", "Gets all roles for a project")]
    public static string GetProjectRoles(
        [McpParameter("Project key or ID")] string projectKey)
    {
        var result = JiraClient.GetAsync<Dictionary<string, string>>($"project/{projectKey}/role").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_project_role", "Gets users/groups assigned to a specific role in a project")]
    public static string GetProjectRole(
        [McpParameter("Project key or ID")] string projectKey,
        [McpParameter("Role ID")] string roleId)
    {
        return JiraClient.GetStringAsync($"project/{projectKey}/role/{roleId}").GetAwaiter().GetResult();
    }
}

