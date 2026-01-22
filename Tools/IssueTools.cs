using JiraMcp.Models;
using JiraMcp.Services;
using MCPServer.Attributes;

namespace JiraMcp.Tools;

/// <summary>
/// MCP Tools for Jira Issue operations (Cloud and Data Center)
/// </summary>
public static class IssueTools
{
    [McpTool("jira_get_issue", "Gets a Jira issue by key or ID with full details including comments, attachments, and changelog")]
    public static string GetIssue(
        [McpParameter("The issue key (e.g., PROJ-123) or issue ID")] string issueKey,
        [McpParameter("Comma-separated list of fields to expand (e.g., changelog,renderedFields)", false)] string? expand = null)
    {
        var endpoint = $"issue/{issueKey}";
        if (!string.IsNullOrEmpty(expand))
        {
            endpoint += $"?expand={expand}";
        }

        var issue = JiraClient.GetAsync<JiraIssue>(endpoint).GetAwaiter().GetResult();
        return JiraClient.ToJson(issue);
    }

    [McpTool("jira_search_issues", "Searches for Jira issues using JQL (Jira Query Language)")]
    public static string SearchIssues(
        [McpParameter("JQL query string (e.g., 'project = PROJ AND status = Open')")] string jql,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return (max 100)", false)] int maxResults = 50,
        [McpParameter("Comma-separated list of fields to return", false)] string? fields = null)
    {
        var endpoint = $"search?jql={Uri.EscapeDataString(jql)}&startAt={startAt}&maxResults={Math.Min(maxResults, 100)}";
        
        if (!string.IsNullOrEmpty(fields))
        {
            endpoint += $"&fields={fields}";
        }

        var result = JiraClient.GetAsync<SearchResult>(endpoint).GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_create_issue", "Creates a new Jira issue")]
    public static string CreateIssue(
        [McpParameter("Project key (e.g., PROJ)")] string projectKey,
        [McpParameter("Issue type name (e.g., Bug, Task, Story)")] string issueType,
        [McpParameter("Issue summary/title")] string summary,
        [McpParameter("Issue description", false)] string? description = null,
        [McpParameter("Assignee account ID (Cloud) or username (Data Center)", false)] string? assignee = null,
        [McpParameter("Priority name (e.g., High, Medium, Low)", false)] string? priority = null,
        [McpParameter("Comma-separated list of labels", false)] string? labels = null,
        [McpParameter("Comma-separated list of component names", false)] string? components = null,
        [McpParameter("Parent issue key for subtasks", false)] string? parentKey = null,
        [McpParameter("Due date in format YYYY-MM-DD", false)] string? dueDate = null,
        [McpParameter("Original time estimate (e.g., 2h, 1d)", false)] string? originalEstimate = null)
    {
        var request = new CreateIssueRequest
        {
            Fields = new CreateIssueFields
            {
                Project = new ProjectRef(Key: projectKey),
                IssueType = new IssueTypeRef(Name: issueType),
                Summary = summary,
                Description = description,
                Assignee = !string.IsNullOrEmpty(assignee) 
                    ? (JiraClient.IsCloud ? new UserRef(AccountId: assignee) : new UserRef(Name: assignee)) 
                    : null,
                Priority = !string.IsNullOrEmpty(priority) ? new PriorityRef(Name: priority) : null,
                Labels = !string.IsNullOrEmpty(labels) 
                    ? labels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() 
                    : null,
                Components = !string.IsNullOrEmpty(components)
                    ? components.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(c => new ComponentRef(Name: c)).ToList()
                    : null,
                Parent = !string.IsNullOrEmpty(parentKey) ? new ParentRef(Key: parentKey) : null,
                DueDate = dueDate,
                TimeTracking = !string.IsNullOrEmpty(originalEstimate) 
                    ? new TimeTrackingInput(OriginalEstimate: originalEstimate) 
                    : null
            }
        };

        var result = JiraClient.PostAsync<CreateIssueResponse>("issue", request).GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_update_issue", "Updates an existing Jira issue")]
    public static string UpdateIssue(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("New summary/title", false)] string? summary = null,
        [McpParameter("New description", false)] string? description = null,
        [McpParameter("New assignee account ID (Cloud) or username (Data Center)", false)] string? assignee = null,
        [McpParameter("New priority name", false)] string? priority = null,
        [McpParameter("New comma-separated list of labels (replaces existing)", false)] string? labels = null,
        [McpParameter("New due date in format YYYY-MM-DD", false)] string? dueDate = null,
        [McpParameter("New original estimate (e.g., 2h, 1d)", false)] string? originalEstimate = null,
        [McpParameter("New remaining estimate (e.g., 1h, 4h)", false)] string? remainingEstimate = null)
    {
        var request = new UpdateIssueRequest
        {
            Fields = new UpdateIssueFields
            {
                Summary = summary,
                Description = description,
                Assignee = !string.IsNullOrEmpty(assignee)
                    ? (JiraClient.IsCloud ? new UserRef(AccountId: assignee) : new UserRef(Name: assignee))
                    : null,
                Priority = !string.IsNullOrEmpty(priority) ? new PriorityRef(Name: priority) : null,
                Labels = !string.IsNullOrEmpty(labels)
                    ? labels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : null,
                DueDate = dueDate,
                TimeTracking = (!string.IsNullOrEmpty(originalEstimate) || !string.IsNullOrEmpty(remainingEstimate))
                    ? new TimeTrackingInput(originalEstimate, remainingEstimate)
                    : null
            }
        };

        JiraClient.PutAsync<object>($"issue/{issueKey}", request).GetAwaiter().GetResult();
        return $"Issue {issueKey} updated successfully";
    }

    [McpTool("jira_delete_issue", "Deletes a Jira issue")]
    public static string DeleteIssue(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Whether to delete subtasks as well", false)] bool deleteSubtasks = false)
    {
        var endpoint = $"issue/{issueKey}";
        if (deleteSubtasks)
        {
            endpoint += "?deleteSubtasks=true";
        }

        JiraClient.DeleteAsync(endpoint).GetAwaiter().GetResult();
        return $"Issue {issueKey} deleted successfully";
    }

    [McpTool("jira_get_transitions", "Gets available status transitions for an issue")]
    public static string GetTransitions(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey)
    {
        var result = JiraClient.GetAsync<TransitionsResult>($"issue/{issueKey}/transitions").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_transition_issue", "Transitions an issue to a new status")]
    public static string TransitionIssue(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("The transition ID (get from jira_get_transitions)")] string transitionId,
        [McpParameter("Resolution name if transitioning to a resolved status", false)] string? resolution = null)
    {
        var request = new
        {
            transition = new { id = transitionId },
            fields = resolution != null ? new { resolution = new { name = resolution } } : null
        };

        JiraClient.PostAsync<object>($"issue/{issueKey}/transitions", request).GetAwaiter().GetResult();
        return $"Issue {issueKey} transitioned successfully";
    }

    [McpTool("jira_assign_issue", "Assigns an issue to a user")]
    public static string AssignIssue(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Account ID (Cloud) or username (Data Center) of assignee. Use '-1' for automatic or null to unassign", false)] string? assignee = null)
    {
        object request;
        
        if (JiraClient.IsCloud)
        {
            request = new { accountId = assignee == "-1" ? "-1" : assignee };
        }
        else
        {
            request = new { name = assignee == "-1" ? "-1" : assignee };
        }

        JiraClient.PutAsync<object>($"issue/{issueKey}/assignee", request).GetAwaiter().GetResult();
        return assignee == null 
            ? $"Issue {issueKey} unassigned" 
            : $"Issue {issueKey} assigned to {assignee}";
    }

    [McpTool("jira_add_labels", "Adds labels to an issue")]
    public static string AddLabels(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Comma-separated list of labels to add")] string labels)
    {
        var labelList = labels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        var request = new
        {
            update = new
            {
                labels = labelList.Select(l => new { add = l }).ToArray()
            }
        };

        JiraClient.PutAsync<object>($"issue/{issueKey}", request).GetAwaiter().GetResult();
        return $"Labels added to issue {issueKey}";
    }

    [McpTool("jira_remove_labels", "Removes labels from an issue")]
    public static string RemoveLabels(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Comma-separated list of labels to remove")] string labels)
    {
        var labelList = labels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        var request = new
        {
            update = new
            {
                labels = labelList.Select(l => new { remove = l }).ToArray()
            }
        };

        JiraClient.PutAsync<object>($"issue/{issueKey}", request).GetAwaiter().GetResult();
        return $"Labels removed from issue {issueKey}";
    }

    [McpTool("jira_link_issues", "Creates a link between two issues")]
    public static string LinkIssues(
        [McpParameter("The type of link (e.g., 'Blocks', 'is blocked by', 'relates to')")] string linkType,
        [McpParameter("The inward issue key (the issue that is affected)")] string inwardIssue,
        [McpParameter("The outward issue key (the issue that causes the effect)")] string outwardIssue,
        [McpParameter("Comment to add to the link", false)] string? comment = null)
    {
        var request = new
        {
            type = new { name = linkType },
            inwardIssue = new { key = inwardIssue },
            outwardIssue = new { key = outwardIssue },
            comment = comment != null ? new { body = comment } : null
        };

        JiraClient.PostAsync<object>("issueLink", request).GetAwaiter().GetResult();
        return $"Linked {inwardIssue} to {outwardIssue} with relationship '{linkType}'";
    }

    [McpTool("jira_get_link_types", "Gets all available issue link types")]
    public static string GetLinkTypes()
    {
        var result = JiraClient.GetAsync<object>("issueLinkType").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_watch_issue", "Adds the current user as a watcher to an issue")]
    public static string WatchIssue(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey)
    {
        JiraClient.PostAsync<object>($"issue/{issueKey}/watchers", null).GetAwaiter().GetResult();
        return $"Now watching issue {issueKey}";
    }

    [McpTool("jira_unwatch_issue", "Removes the current user from watchers of an issue")]
    public static string UnwatchIssue(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Account ID (Cloud) or username (Data Center) to remove. Defaults to current user.", false)] string? user = null)
    {
        var endpoint = $"issue/{issueKey}/watchers";
        if (!string.IsNullOrEmpty(user))
        {
            endpoint += $"?{(JiraClient.IsCloud ? "accountId" : "username")}={user}";
        }
        
        JiraClient.DeleteAsync(endpoint).GetAwaiter().GetResult();
        return $"Stopped watching issue {issueKey}";
    }

    [McpTool("jira_get_watchers", "Gets the list of watchers for an issue")]
    public static string GetWatchers(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey)
    {
        var result = JiraClient.GetAsync<object>($"issue/{issueKey}/watchers").GetAwaiter().GetResult();
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_vote_issue", "Adds a vote to an issue")]
    public static string VoteIssue(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey)
    {
        JiraClient.PostAsync<object>($"issue/{issueKey}/votes", null).GetAwaiter().GetResult();
        return $"Voted for issue {issueKey}";
    }

    [McpTool("jira_unvote_issue", "Removes a vote from an issue")]
    public static string UnvoteIssue(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey)
    {
        JiraClient.DeleteAsync($"issue/{issueKey}/votes").GetAwaiter().GetResult();
        return $"Removed vote from issue {issueKey}";
    }

    [McpTool("jira_get_changelog", "Gets the changelog/history of an issue")]
    public static string GetChangelog(
        [McpParameter("The issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 100)
    {
        var issue = JiraClient.GetAsync<JiraIssue>(
            $"issue/{issueKey}?expand=changelog&startAt={startAt}&maxResults={maxResults}").GetAwaiter().GetResult();
        return JiraClient.ToJson(issue?.Changelog);
    }
}
