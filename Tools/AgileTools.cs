using JiraMcp.Models;
using JiraMcp.Services;
using MCPServer.Attributes;

namespace JiraMcp.Tools;

/// <summary>
/// MCP Tools for Jira Agile operations - Boards, Sprints, Epics, Backlog (Cloud and Data Center)
/// </summary>
public static class AgileTools
{
    #region Board Operations

    [McpTool("jira_list_boards", "Lists all agile boards")]
    public static string ListBoards(
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("Filter by board type (scrum, kanban)", false)] string? type = null,
        [McpParameter("Filter by board name containing this string", false)] string? name = null,
        [McpParameter("Filter by project key or ID", false)] string? projectKeyOrId = null)
    {
        var endpoint = $"board?startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(type))
        {
            endpoint += $"&type={type}";
        }
        
        if (!string.IsNullOrEmpty(name))
        {
            endpoint += $"&name={Uri.EscapeDataString(name)}";
        }
        
        if (!string.IsNullOrEmpty(projectKeyOrId))
        {
            endpoint += $"&projectKeyOrId={Uri.EscapeDataString(projectKeyOrId)}";
        }

        return JiraClient.GetAgileStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_get_board", "Gets details of a specific board")]
    public static string GetBoard(
        [McpParameter("Board ID")] int boardId)
    {
        return JiraClient.GetAgileStringAsync($"board/{boardId}").GetAwaiter().GetResult();
    }

    [McpTool("jira_get_board_configuration", "Gets the configuration of a board")]
    public static string GetBoardConfiguration(
        [McpParameter("Board ID")] int boardId)
    {
        return JiraClient.GetAgileStringAsync($"board/{boardId}/configuration").GetAwaiter().GetResult();
    }

    [McpTool("jira_get_board_issues", "Gets all issues on a board")]
    public static string GetBoardIssues(
        [McpParameter("Board ID")] int boardId,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("JQL to filter issues", false)] string? jql = null,
        [McpParameter("Comma-separated list of fields to return", false)] string? fields = null)
    {
        var endpoint = $"board/{boardId}/issue?startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(jql))
        {
            endpoint += $"&jql={Uri.EscapeDataString(jql)}";
        }
        
        if (!string.IsNullOrEmpty(fields))
        {
            endpoint += $"&fields={fields}";
        }

        return JiraClient.GetAgileStringAsync(endpoint).GetAwaiter().GetResult();
    }

    #endregion

    #region Sprint Operations

    [McpTool("jira_list_sprints", "Lists all sprints for a board")]
    public static string ListSprints(
        [McpParameter("Board ID")] int boardId,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("Filter by state (future, active, closed)", false)] string? state = null)
    {
        var endpoint = $"board/{boardId}/sprint?startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(state))
        {
            endpoint += $"&state={state}";
        }

        return JiraClient.GetAgileStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_get_sprint", "Gets details of a specific sprint")]
    public static string GetSprint(
        [McpParameter("Sprint ID")] int sprintId)
    {
        return JiraClient.GetAgileStringAsync($"sprint/{sprintId}").GetAwaiter().GetResult();
    }

    [McpTool("jira_create_sprint", "Creates a new sprint")]
    public static string CreateSprint(
        [McpParameter("Sprint name")] string name,
        [McpParameter("Board ID to create sprint on")] int originBoardId,
        [McpParameter("Sprint start date (ISO format)", false)] string? startDate = null,
        [McpParameter("Sprint end date (ISO format)", false)] string? endDate = null,
        [McpParameter("Sprint goal", false)] string? goal = null)
    {
        var request = new Dictionary<string, object?>
        {
            ["name"] = name,
            ["originBoardId"] = originBoardId
        };
        
        if (!string.IsNullOrEmpty(startDate)) request["startDate"] = startDate;
        if (!string.IsNullOrEmpty(endDate)) request["endDate"] = endDate;
        if (!string.IsNullOrEmpty(goal)) request["goal"] = goal;

        return JiraClient.PostAgileStringAsync("sprint", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_update_sprint", "Updates a sprint")]
    public static string UpdateSprint(
        [McpParameter("Sprint ID")] int sprintId,
        [McpParameter("New sprint name", false)] string? name = null,
        [McpParameter("New state (future, active, closed)", false)] string? state = null,
        [McpParameter("New start date (ISO format)", false)] string? startDate = null,
        [McpParameter("New end date (ISO format)", false)] string? endDate = null,
        [McpParameter("New sprint goal", false)] string? goal = null)
    {
        var request = new Dictionary<string, object?>();
        
        if (!string.IsNullOrEmpty(name)) request["name"] = name;
        if (!string.IsNullOrEmpty(state)) request["state"] = state;
        if (!string.IsNullOrEmpty(startDate)) request["startDate"] = startDate;
        if (!string.IsNullOrEmpty(endDate)) request["endDate"] = endDate;
        if (!string.IsNullOrEmpty(goal)) request["goal"] = goal;

        // Agile API uses POST for partial updates
        return JiraClient.PostAgileStringAsync($"sprint/{sprintId}", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_start_sprint", "Starts a sprint")]
    public static string StartSprint(
        [McpParameter("Sprint ID")] int sprintId,
        [McpParameter("Sprint end date (ISO format). Required.")] string endDate,
        [McpParameter("Sprint start date (ISO format). Defaults to now.", false)] string? startDate = null)
    {
        var request = new Dictionary<string, object?>
        {
            ["state"] = "active",
            ["startDate"] = startDate ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            ["endDate"] = endDate
        };

        return JiraClient.PostAgileStringAsync($"sprint/{sprintId}", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_close_sprint", "Closes/completes a sprint")]
    public static string CloseSprint(
        [McpParameter("Sprint ID")] int sprintId,
        [McpParameter("Complete date (ISO format). Defaults to now.", false)] string? completeDate = null)
    {
        var request = new Dictionary<string, object?>
        {
            ["state"] = "closed",
            ["completeDate"] = completeDate ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };

        return JiraClient.PostAgileStringAsync($"sprint/{sprintId}", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_delete_sprint", "Deletes a sprint")]
    public static string DeleteSprint(
        [McpParameter("Sprint ID")] int sprintId)
    {
        JiraClient.DeleteAgileAsync($"sprint/{sprintId}").GetAwaiter().GetResult();
        return $"Sprint {sprintId} deleted";
    }

    [McpTool("jira_get_sprint_issues", "Gets all issues in a sprint")]
    public static string GetSprintIssues(
        [McpParameter("Sprint ID")] int sprintId,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("JQL to filter issues", false)] string? jql = null,
        [McpParameter("Comma-separated list of fields to return", false)] string? fields = null)
    {
        var endpoint = $"sprint/{sprintId}/issue?startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(jql))
        {
            endpoint += $"&jql={Uri.EscapeDataString(jql)}";
        }
        
        if (!string.IsNullOrEmpty(fields))
        {
            endpoint += $"&fields={fields}";
        }

        return JiraClient.GetAgileStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_move_issues_to_sprint", "Moves issues to a sprint")]
    public static string MoveIssuesToSprint(
        [McpParameter("Sprint ID")] int sprintId,
        [McpParameter("Comma-separated list of issue keys to move")] string issueKeys)
    {
        var keys = issueKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        
        var request = new { issues = keys };
        JiraClient.PostAgileAsync<object>($"sprint/{sprintId}/issue", request).GetAwaiter().GetResult();
        return $"Moved {keys.Count} issue(s) to sprint {sprintId}";
    }

    [McpTool("jira_get_sprint_report", "Gets the sprint report for a completed sprint")]
    public static string GetSprintReport(
        [McpParameter("Board ID")] int boardId,
        [McpParameter("Sprint ID")] int sprintId)
    {
        // Note: This endpoint is available in Jira Software but not in the standard Agile API
        return JiraClient.GetStringAsync(
            $"/rest/greenhopper/1.0/rapid/charts/sprintreport?rapidViewId={boardId}&sprintId={sprintId}").GetAwaiter().GetResult();
    }

    #endregion

    #region Backlog Operations

    [McpTool("jira_get_backlog", "Gets issues in the backlog for a board")]
    public static string GetBacklog(
        [McpParameter("Board ID")] int boardId,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("JQL to filter issues", false)] string? jql = null,
        [McpParameter("Comma-separated list of fields to return", false)] string? fields = null)
    {
        var endpoint = $"board/{boardId}/backlog?startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(jql))
        {
            endpoint += $"&jql={Uri.EscapeDataString(jql)}";
        }
        
        if (!string.IsNullOrEmpty(fields))
        {
            endpoint += $"&fields={fields}";
        }

        return JiraClient.GetAgileStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_move_issues_to_backlog", "Moves issues to the backlog")]
    public static string MoveIssuesToBacklog(
        [McpParameter("Comma-separated list of issue keys to move to backlog")] string issueKeys)
    {
        var keys = issueKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        
        var request = new { issues = keys };
        JiraClient.PostAgileAsync<object>("backlog/issue", request).GetAwaiter().GetResult();
        return $"Moved {keys.Count} issue(s) to backlog";
    }

    #endregion

    #region Epic Operations

    [McpTool("jira_list_epics", "Lists all epics on a board")]
    public static string ListEpics(
        [McpParameter("Board ID")] int boardId,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("Filter by done status (true/false)", false)] bool? done = null)
    {
        var endpoint = $"board/{boardId}/epic?startAt={startAt}&maxResults={maxResults}";
        
        if (done.HasValue)
        {
            endpoint += $"&done={done.Value.ToString().ToLower()}";
        }

        return JiraClient.GetAgileStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_get_epic", "Gets details of a specific epic")]
    public static string GetEpic(
        [McpParameter("Epic ID or key")] string epicIdOrKey)
    {
        return JiraClient.GetAgileStringAsync($"epic/{epicIdOrKey}").GetAwaiter().GetResult();
    }

    [McpTool("jira_get_epic_issues", "Gets all issues belonging to an epic")]
    public static string GetEpicIssues(
        [McpParameter("Epic ID or key")] string epicIdOrKey,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("JQL to filter issues", false)] string? jql = null,
        [McpParameter("Comma-separated list of fields to return", false)] string? fields = null)
    {
        var endpoint = $"epic/{epicIdOrKey}/issue?startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(jql))
        {
            endpoint += $"&jql={Uri.EscapeDataString(jql)}";
        }
        
        if (!string.IsNullOrEmpty(fields))
        {
            endpoint += $"&fields={fields}";
        }

        return JiraClient.GetAgileStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_move_issues_to_epic", "Moves issues to an epic")]
    public static string MoveIssuesToEpic(
        [McpParameter("Epic ID or key")] string epicIdOrKey,
        [McpParameter("Comma-separated list of issue keys to move")] string issueKeys)
    {
        var keys = issueKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        
        var request = new { issues = keys };
        JiraClient.PostAgileAsync<object>($"epic/{epicIdOrKey}/issue", request).GetAwaiter().GetResult();
        return $"Moved {keys.Count} issue(s) to epic {epicIdOrKey}";
    }

    [McpTool("jira_remove_issues_from_epic", "Removes issues from their epic (moves to backlog)")]
    public static string RemoveIssuesFromEpic(
        [McpParameter("Comma-separated list of issue keys to remove from epic")] string issueKeys)
    {
        var keys = issueKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        
        var request = new { issues = keys };
        JiraClient.PostAgileAsync<object>("epic/none/issue", request).GetAwaiter().GetResult();
        return $"Removed {keys.Count} issue(s) from their epic";
    }

    [McpTool("jira_rank_epics", "Ranks an epic relative to another epic")]
    public static string RankEpics(
        [McpParameter("Epic ID or key to rank")] string epicIdOrKey,
        [McpParameter("Epic ID or key to rank before", false)] string? rankBeforeEpic = null,
        [McpParameter("Epic ID or key to rank after", false)] string? rankAfterEpic = null)
    {
        var request = new Dictionary<string, object?>();
        
        if (!string.IsNullOrEmpty(rankBeforeEpic))
        {
            request["rankBeforeEpic"] = rankBeforeEpic;
        }
        else if (!string.IsNullOrEmpty(rankAfterEpic))
        {
            request["rankAfterEpic"] = rankAfterEpic;
        }

        JiraClient.PutAsync<object>($"/rest/agile/1.0/epic/{epicIdOrKey}/rank", request).GetAwaiter().GetResult();
        return $"Epic {epicIdOrKey} ranked successfully";
    }

    #endregion

    #region Issue Ranking

    [McpTool("jira_rank_issues", "Ranks issues relative to each other")]
    public static string RankIssues(
        [McpParameter("Comma-separated list of issue keys to rank")] string issueKeys,
        [McpParameter("Issue key to rank before", false)] string? rankBeforeIssue = null,
        [McpParameter("Issue key to rank after", false)] string? rankAfterIssue = null)
    {
        var keys = issueKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        
        var request = new Dictionary<string, object?>
        {
            ["issues"] = keys
        };
        
        if (!string.IsNullOrEmpty(rankBeforeIssue))
        {
            request["rankBeforeIssue"] = rankBeforeIssue;
        }
        else if (!string.IsNullOrEmpty(rankAfterIssue))
        {
            request["rankAfterIssue"] = rankAfterIssue;
        }

        JiraClient.PutAsync<object>("/rest/agile/1.0/issue/rank", request).GetAwaiter().GetResult();
        return $"Ranked {keys.Count} issue(s) successfully";
    }

    #endregion

    #region Velocity and Burndown

    [McpTool("jira_get_velocity_chart", "Gets velocity chart data for a board")]
    public static string GetVelocityChart(
        [McpParameter("Board ID")] int boardId)
    {
        // Note: This uses the Greenhopper API which may not be available on all installations
        return JiraClient.GetStringAsync(
            $"/rest/greenhopper/1.0/rapid/charts/velocity?rapidViewId={boardId}").GetAwaiter().GetResult();
    }

    [McpTool("jira_get_burndown_chart", "Gets burndown chart data for a sprint")]
    public static string GetBurndownChart(
        [McpParameter("Board ID")] int boardId,
        [McpParameter("Sprint ID")] int sprintId)
    {
        // Note: This uses the Greenhopper API which may not be available on all installations
        return JiraClient.GetStringAsync(
            $"/rest/greenhopper/1.0/rapid/charts/scopechangeburndownchart?rapidViewId={boardId}&sprintId={sprintId}").GetAwaiter().GetResult();
    }

    #endregion
}


