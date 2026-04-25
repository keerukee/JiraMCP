using JiraMcp.Models;
using JiraMcp.Services;
using MCPServer.Attributes;

namespace JiraMcp.Tools;

/// <summary>
/// MCP Tools for Jira Comment operations (Cloud and Data Center)
/// </summary>
public static class CommentTools
{
    [McpTool("jira_get_comments", "Gets all comments on an issue")]
    public static string GetComments(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("Order by created date ('created' for ascending, '-created' for descending)", false)] string? orderBy = null,
        [McpParameter("Expand rendered body", false)] bool expand = false)
    {
        var endpoint = $"issue/{issueKey}/comment?startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(orderBy))
        {
            endpoint += $"&orderBy={orderBy}";
        }
        
        if (expand)
        {
            endpoint += "&expand=renderedBody";
        }

        var result = JiraClient.GetAsync<CommentContainer>(endpoint).GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_get_comment", "Gets a specific comment by ID")]
    public static string GetComment(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Comment ID")] string commentId,
        [McpParameter("Expand rendered body", false)] bool expand = false)
    {
        var endpoint = $"issue/{issueKey}/comment/{commentId}";
        
        if (expand)
        {
            endpoint += "?expand=renderedBody";
        }

        var result = JiraClient.GetAsync<JiraComment>(endpoint).GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_add_comment", "Adds a comment to an issue. For Cloud V3, body uses Atlassian Document Format (ADF) automatically.")]
    public static string AddComment(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Comment body text (plain text, auto-converted to ADF for Cloud)")] string body,
        [McpParameter("Visibility type ('role' or 'group')", false)] string? visibilityType = null,
        [McpParameter("Visibility value (role name or group name)", false)] string? visibilityValue = null)
    {
        var request = new Dictionary<string, object?>
        {
            // Cloud V3: convert to ADF; Data Center V2: keep as plain text
            ["body"] = JiraClient.FormatTextForApi(body)
        };
        
        if (!string.IsNullOrEmpty(visibilityType) && !string.IsNullOrEmpty(visibilityValue))
        {
            request["visibility"] = new
            {
                type = visibilityType,
                value = visibilityValue
            };
        }

        var result = JiraClient.PostAsync<JiraComment>($"issue/{issueKey}/comment", request).GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_update_comment", "Updates an existing comment. For Cloud V3, body uses Atlassian Document Format (ADF) automatically.")]
    public static string UpdateComment(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Comment ID")] string commentId,
        [McpParameter("New comment body text (plain text, auto-converted to ADF for Cloud)")] string body,
        [McpParameter("Visibility type ('role' or 'group')", false)] string? visibilityType = null,
        [McpParameter("Visibility value (role name or group name)", false)] string? visibilityValue = null)
    {
        var request = new Dictionary<string, object?>
        {
            // Cloud V3: convert to ADF; Data Center V2: keep as plain text
            ["body"] = JiraClient.FormatTextForApi(body)
        };
        
        if (!string.IsNullOrEmpty(visibilityType) && !string.IsNullOrEmpty(visibilityValue))
        {
            request["visibility"] = new
            {
                type = visibilityType,
                value = visibilityValue
            };
        }

        var result = JiraClient.PutAsync<JiraComment>($"issue/{issueKey}/comment/{commentId}", request).GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_delete_comment", "Deletes a comment from an issue")]
    public static string DeleteComment(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Comment ID")] string commentId)
    {
        JiraClient.DeleteAsync($"issue/{issueKey}/comment/{commentId}").GetAwaiter().GetResult();
        return $"Comment {commentId} deleted from issue {issueKey}";
    }
}
