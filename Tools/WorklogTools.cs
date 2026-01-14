using JiraMcp.Models;
using JiraMcp.Services;
using MCPServer.Attributes;

namespace JiraMcp.Tools;

/// <summary>
/// MCP Tools for Jira Worklog/Time Tracking operations (Cloud and Data Center)
/// </summary>
public static class WorklogTools
{
    [McpTool("jira_get_worklogs", "Gets all worklogs on an issue")]
    public static async Task<string> GetWorklogs(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 100)
    {
        var endpoint = $"issue/{issueKey}/worklog?startAt={startAt}&maxResults={maxResults}";
        var result = await JiraClient.GetAsync<WorklogContainer>(endpoint);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_worklog", "Gets a specific worklog by ID")]
    public static async Task<string> GetWorklog(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Worklog ID")] string worklogId)
    {
        var result = await JiraClient.GetAsync<JiraWorklog>($"issue/{issueKey}/worklog/{worklogId}");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_add_worklog", "Adds a worklog entry to an issue")]
    public static async Task<string> AddWorklog(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Time spent (e.g., '2h', '1d', '30m')")] string timeSpent,
        [McpParameter("When the work was started (ISO format). Defaults to now.", false)] string? started = null,
        [McpParameter("Comment describing the work done", false)] string? comment = null,
        [McpParameter("How to adjust remaining estimate: 'new' (set to newEstimate), 'leave' (don't change), 'manual' (reduce by reduceBy), 'auto' (reduce by timeSpent)", false)] string? adjustEstimate = null,
        [McpParameter("New estimate value when adjustEstimate='new' (e.g., '1h')", false)] string? newEstimate = null,
        [McpParameter("Reduce remaining estimate by this amount when adjustEstimate='manual' (e.g., '30m')", false)] string? reduceBy = null)
    {
        var request = new Dictionary<string, object?>
        {
            ["timeSpent"] = timeSpent,
            ["started"] = started ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
        
        if (!string.IsNullOrEmpty(comment))
        {
            request["comment"] = comment;
        }

        var endpoint = $"issue/{issueKey}/worklog";
        
        if (!string.IsNullOrEmpty(adjustEstimate))
        {
            endpoint += $"?adjustEstimate={adjustEstimate}";
            
            if (adjustEstimate == "new" && !string.IsNullOrEmpty(newEstimate))
            {
                endpoint += $"&newEstimate={newEstimate}";
            }
            else if (adjustEstimate == "manual" && !string.IsNullOrEmpty(reduceBy))
            {
                endpoint += $"&reduceBy={reduceBy}";
            }
        }

        var result = await JiraClient.PostAsync<JiraWorklog>(endpoint, request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_update_worklog", "Updates an existing worklog entry")]
    public static async Task<string> UpdateWorklog(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Worklog ID")] string worklogId,
        [McpParameter("New time spent (e.g., '2h', '1d', '30m')", false)] string? timeSpent = null,
        [McpParameter("New start time (ISO format)", false)] string? started = null,
        [McpParameter("New comment describing the work done", false)] string? comment = null,
        [McpParameter("How to adjust remaining estimate: 'new', 'leave', 'manual', 'auto'", false)] string? adjustEstimate = null,
        [McpParameter("New estimate value when adjustEstimate='new'", false)] string? newEstimate = null)
    {
        var request = new Dictionary<string, object?>();
        
        if (!string.IsNullOrEmpty(timeSpent)) request["timeSpent"] = timeSpent;
        if (!string.IsNullOrEmpty(started)) request["started"] = started;
        if (!string.IsNullOrEmpty(comment)) request["comment"] = comment;

        var endpoint = $"issue/{issueKey}/worklog/{worklogId}";
        
        if (!string.IsNullOrEmpty(adjustEstimate))
        {
            endpoint += $"?adjustEstimate={adjustEstimate}";
            
            if (adjustEstimate == "new" && !string.IsNullOrEmpty(newEstimate))
            {
                endpoint += $"&newEstimate={newEstimate}";
            }
        }

        var result = await JiraClient.PutAsync<JiraWorklog>(endpoint, request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_delete_worklog", "Deletes a worklog entry")]
    public static async Task<string> DeleteWorklog(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Worklog ID")] string worklogId,
        [McpParameter("How to adjust remaining estimate: 'new', 'leave', 'manual', 'auto'", false)] string? adjustEstimate = null,
        [McpParameter("New estimate value when adjustEstimate='new'", false)] string? newEstimate = null,
        [McpParameter("Increase remaining estimate by this amount when adjustEstimate='manual'", false)] string? increaseBy = null)
    {
        var endpoint = $"issue/{issueKey}/worklog/{worklogId}";
        
        if (!string.IsNullOrEmpty(adjustEstimate))
        {
            endpoint += $"?adjustEstimate={adjustEstimate}";
            
            if (adjustEstimate == "new" && !string.IsNullOrEmpty(newEstimate))
            {
                endpoint += $"&newEstimate={newEstimate}";
            }
            else if (adjustEstimate == "manual" && !string.IsNullOrEmpty(increaseBy))
            {
                endpoint += $"&increaseBy={increaseBy}";
            }
        }

        await JiraClient.DeleteAsync(endpoint);
        return $"Worklog {worklogId} deleted from issue {issueKey}";
    }

    [McpTool("jira_get_worklogs_for_ids", "Gets worklogs by their IDs (bulk operation)")]
    public static async Task<string> GetWorklogsByIds(
        [McpParameter("Comma-separated list of worklog IDs")] string worklogIds)
    {
        var ids = worklogIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(long.Parse)
            .ToList();
        
        var request = new { ids };
        var result = await JiraClient.PostAsync<List<JiraWorklog>>("worklog/list", request);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_worklogs_updated_since", "Gets worklogs that have been updated since a specific time")]
    public static async Task<string> GetWorklogsUpdatedSince(
        [McpParameter("Unix timestamp in milliseconds to get updates since")] long since)
    {
        var result = await JiraClient.GetAsync<object>($"worklog/updated?since={since}");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_time_tracking", "Gets the time tracking information for an issue")]
    public static async Task<string> GetTimeTracking(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey)
    {
        var issue = await JiraClient.GetAsync<JiraIssue>($"issue/{issueKey}?fields=timetracking");
        return JiraClient.ToJson(issue?.Fields?.TimeTracking);
    }

    [McpTool("jira_set_time_estimate", "Sets the original or remaining time estimate for an issue")]
    public static async Task<string> SetTimeEstimate(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Original estimate (e.g., '2d', '8h')", false)] string? originalEstimate = null,
        [McpParameter("Remaining estimate (e.g., '1d', '4h')", false)] string? remainingEstimate = null)
    {
        var timeTracking = new Dictionary<string, object?>();
        
        if (!string.IsNullOrEmpty(originalEstimate))
        {
            timeTracking["originalEstimate"] = originalEstimate;
        }
        
        if (!string.IsNullOrEmpty(remainingEstimate))
        {
            timeTracking["remainingEstimate"] = remainingEstimate;
        }

        var request = new
        {
            fields = new { timetracking = timeTracking }
        };

        await JiraClient.PutAsync<object>($"issue/{issueKey}", request);
        return $"Time estimate updated for issue {issueKey}";
    }
}
