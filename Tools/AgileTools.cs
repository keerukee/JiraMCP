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
    public static async Task<string> ListBoards(
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

        var result = await JiraClient.GetAgileAsync<BoardsResult>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_board", "Gets details of a specific board")]
    public static async Task<string> GetBoard(
        [McpParameter("Board ID")] int boardId)
    {
        var result = await JiraClient.GetAgileAsync<JiraBoard>($"board/{boardId}");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_board_configuration", "Gets the configuration of a board")]
    public static async Task<string> GetBoardConfiguration(
        [McpParameter("Board ID")] int boardId)
    {
        var result = await JiraClient.GetAgileAsync<object>($"board/{boardId}/configuration");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_board_issues", "Gets all issues on a board")]
    public static async Task<string> GetBoardIssues(
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

        var result = await JiraClient.GetAgileAsync<SprintIssuesResult>(endpoint);
        return JiraClient.ToJson(result);
    }

    #endregion

    #region Sprint Operations

    [McpTool("jira_list_sprints", "Lists all sprints for a board")]
    public static async Task<string> ListSprints(
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

        var result = await JiraClient.GetAgileAsync<SprintsResult>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_sprint", "Gets details of a specific sprint")]
    public static async Task<string> GetSprint(
        [McpParameter("Sprint ID")] int sprintId)
    {
        var result = await JiraClient.GetAgileAsync<JiraSprint>($"sprint/{sprintId}");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_create_sprint", "Creates a new sprint")]
    public static async Task<string> CreateSprint(
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

        var result = await JiraClient.PostAgileAsync<JiraSprint>("sprint", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_update_sprint", "Updates a sprint")]
    public static async Task<string> UpdateSprint(
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
        var result = await JiraClient.PostAgileAsync<JiraSprint>($"sprint/{sprintId}", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_start_sprint", "Starts a sprint")]
    public static async Task<string> StartSprint(
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

        var result = await JiraClient.PostAgileAsync<JiraSprint>($"sprint/{sprintId}", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_close_sprint", "Closes/completes a sprint")]
    public static async Task<string> CloseSprint(
        [McpParameter("Sprint ID")] int sprintId,
        [McpParameter("Complete date (ISO format). Defaults to now.", false)] string? completeDate = null)
    {
        var request = new Dictionary<string, object?>
        {
            ["state"] = "closed",
            ["completeDate"] = completeDate ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };

        var result = await JiraClient.PostAgileAsync<JiraSprint>($"sprint/{sprintId}", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_delete_sprint", "Deletes a sprint")]
    public static async Task<string> DeleteSprint(
        [McpParameter("Sprint ID")] int sprintId)
    {
        await JiraClient.DeleteAgileAsync($"sprint/{sprintId}");
        return $"Sprint {sprintId} deleted";
    }

    [McpTool("jira_get_sprint_issues", "Gets all issues in a sprint")]
    public static async Task<string> GetSprintIssues(
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

        var result = await JiraClient.GetAgileAsync<SprintIssuesResult>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_move_issues_to_sprint", "Moves issues to a sprint")]
    public static async Task<string> MoveIssuesToSprint(
        [McpParameter("Sprint ID")] int sprintId,
        [McpParameter("Comma-separated list of issue keys to move")] string issueKeys)
    {
        var keys = issueKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        
        var request = new { issues = keys };
        await JiraClient.PostAgileAsync<object>($"sprint/{sprintId}/issue", request);
        return $"Moved {keys.Count} issue(s) to sprint {sprintId}";
    }

    [McpTool("jira_get_sprint_report", "Gets the sprint report for a completed sprint")]
    public static async Task<string> GetSprintReport(
        [McpParameter("Board ID")] int boardId,
        [McpParameter("Sprint ID")] int sprintId)
    {
        // Note: This endpoint is available in Jira Software but not in the standard Agile API
        var result = await JiraClient.GetAsync<object>(
            $"/rest/greenhopper/1.0/rapid/charts/sprintreport?rapidViewId={boardId}&sprintId={sprintId}");
        return JiraClient.ToJson(result);
    }

    #endregion

    #region Backlog Operations

    [McpTool("jira_get_backlog", "Gets issues in the backlog for a board")]
    public static async Task<string> GetBacklog(
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

        var result = await JiraClient.GetAgileAsync<BacklogIssuesResult>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_move_issues_to_backlog", "Moves issues to the backlog")]
    public static async Task<string> MoveIssuesToBacklog(
        [McpParameter("Comma-separated list of issue keys to move to backlog")] string issueKeys)
    {
        var keys = issueKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        
        var request = new { issues = keys };
        await JiraClient.PostAgileAsync<object>("backlog/issue", request);
        return $"Moved {keys.Count} issue(s) to backlog";
    }

    #endregion

    #region Epic Operations

    [McpTool("jira_list_epics", "Lists all epics on a board")]
    public static async Task<string> ListEpics(
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

        var result = await JiraClient.GetAgileAsync<object>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_epic", "Gets details of a specific epic")]
    public static async Task<string> GetEpic(
        [McpParameter("Epic ID or key")] string epicIdOrKey)
    {
        var result = await JiraClient.GetAgileAsync<JiraEpic>($"epic/{epicIdOrKey}");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_epic_issues", "Gets all issues belonging to an epic")]
    public static async Task<string> GetEpicIssues(
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

        var result = await JiraClient.GetAgileAsync<SprintIssuesResult>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_move_issues_to_epic", "Moves issues to an epic")]
    public static async Task<string> MoveIssuesToEpic(
        [McpParameter("Epic ID or key")] string epicIdOrKey,
        [McpParameter("Comma-separated list of issue keys to move")] string issueKeys)
    {
        var keys = issueKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        
        var request = new { issues = keys };
        await JiraClient.PostAgileAsync<object>($"epic/{epicIdOrKey}/issue", request);
        return $"Moved {keys.Count} issue(s) to epic {epicIdOrKey}";
    }

    [McpTool("jira_remove_issues_from_epic", "Removes issues from their epic (moves to backlog)")]
    public static async Task<string> RemoveIssuesFromEpic(
        [McpParameter("Comma-separated list of issue keys to remove from epic")] string issueKeys)
    {
        var keys = issueKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        
        var request = new { issues = keys };
        await JiraClient.PostAgileAsync<object>("epic/none/issue", request);
        return $"Removed {keys.Count} issue(s) from their epic";
    }

    [McpTool("jira_rank_epics", "Ranks an epic relative to another epic")]
    public static async Task<string> RankEpics(
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

        await JiraClient.PutAsync<object>($"/rest/agile/1.0/epic/{epicIdOrKey}/rank", request);
        return $"Epic {epicIdOrKey} ranked successfully";
    }

    #endregion

    #region Issue Ranking

    [McpTool("jira_rank_issues", "Ranks issues relative to each other")]
    public static async Task<string> RankIssues(
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

        await JiraClient.PutAsync<object>("/rest/agile/1.0/issue/rank", request);
        return $"Ranked {keys.Count} issue(s) successfully";
    }

    #endregion

    #region Velocity and Burndown

    [McpTool("jira_get_velocity_chart", "Gets velocity chart data for a board")]
    public static async Task<string> GetVelocityChart(
        [McpParameter("Board ID")] int boardId)
    {
        // Note: This uses the Greenhopper API which may not be available on all installations
        var result = await JiraClient.GetAsync<object>(
            $"/rest/greenhopper/1.0/rapid/charts/velocity?rapidViewId={boardId}");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_burndown_chart", "Gets burndown chart data for a sprint")]
    public static async Task<string> GetBurndownChart(
        [McpParameter("Board ID")] int boardId,
        [McpParameter("Sprint ID")] int sprintId)
    {
        // Note: This uses the Greenhopper API which may not be available on all installations
        var result = await JiraClient.GetAsync<object>(
            $"/rest/greenhopper/1.0/rapid/charts/scopechangeburndownchart?rapidViewId={boardId}&sprintId={sprintId}");
        return JiraClient.ToJson(result);
    }

    #endregion
}
