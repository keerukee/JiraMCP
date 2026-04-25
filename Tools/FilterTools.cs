using JiraMcp.Models;
using JiraMcp.Services;
using MCPServer.Attributes;

namespace JiraMcp.Tools;

/// <summary>
/// MCP Tools for Jira Filter operations (Cloud and Data Center)
/// </summary>
public static class FilterTools
{
    [McpTool("jira_list_filters", "Lists filters owned by or shared with the current user")]
    public static string ListFilters(
        [McpParameter("Filter by filter name containing this string", false)] string? filterName = null,
        [McpParameter("Filter by owner account ID (Cloud) or username (Data Center)", false)] string? owner = null,
        [McpParameter("Starting index for pagination", false)] int startAt = 0,
        [McpParameter("Maximum results to return", false)] int maxResults = 50,
        [McpParameter("Comma-separated fields to expand (e.g., description,owner,jql,sharePermissions)", false)] string? expand = null)
    {
        var endpoint = $"filter/search?startAt={startAt}&maxResults={maxResults}";
        
        if (!string.IsNullOrEmpty(filterName))
        {
            endpoint += $"&filterName={Uri.EscapeDataString(filterName)}";
        }
        
        if (!string.IsNullOrEmpty(owner))
        {
            if (JiraClient.IsCloud)
            {
                endpoint += $"&accountId={Uri.EscapeDataString(owner)}";
            }
            else
            {
                endpoint += $"&owner={Uri.EscapeDataString(owner)}";
            }
        }
        
        if (!string.IsNullOrEmpty(expand))
        {
            endpoint += $"&expand={expand}";
        }

        return JiraClient.GetStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_get_filter", "Gets a specific filter by ID")]
    public static string GetFilter(
        [McpParameter("Filter ID")] string filterId,
        [McpParameter("Comma-separated fields to expand", false)] string? expand = null)
    {
        var endpoint = $"filter/{filterId}";
        
        if (!string.IsNullOrEmpty(expand))
        {
            endpoint += $"?expand={expand}";
        }

        return JiraClient.GetStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_create_filter", "Creates a new filter")]
    public static string CreateFilter(
        [McpParameter("Filter name")] string name,
        [McpParameter("JQL query for the filter")] string jql,
        [McpParameter("Filter description", false)] string? description = null,
        [McpParameter("Add to favorites", false)] bool favourite = false)
    {
        var request = new Dictionary<string, object?>
        {
            ["name"] = name,
            ["jql"] = jql,
            ["favourite"] = favourite
        };
        
        if (!string.IsNullOrEmpty(description))
        {
            request["description"] = description;
        }

        return JiraClient.PostStringAsync("filter", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_update_filter", "Updates an existing filter")]
    public static string UpdateFilter(
        [McpParameter("Filter ID")] string filterId,
        [McpParameter("New filter name", false)] string? name = null,
        [McpParameter("New JQL query", false)] string? jql = null,
        [McpParameter("New description", false)] string? description = null,
        [McpParameter("Add/remove from favorites", false)] bool? favourite = null)
    {
        var request = new Dictionary<string, object?>();
        
        if (!string.IsNullOrEmpty(name)) request["name"] = name;
        if (!string.IsNullOrEmpty(jql)) request["jql"] = jql;
        if (!string.IsNullOrEmpty(description)) request["description"] = description;
        if (favourite.HasValue) request["favourite"] = favourite.Value;

        return JiraClient.PutStringAsync($"filter/{filterId}", request).GetAwaiter().GetResult();
    }

    [McpTool("jira_delete_filter", "Deletes a filter")]
    public static string DeleteFilter(
        [McpParameter("Filter ID")] string filterId)
    {
        JiraClient.DeleteAsync($"filter/{filterId}").GetAwaiter().GetResult();
        return $"Filter {filterId} deleted";
    }

    [McpTool("jira_get_favourite_filters", "Gets the current user's favourite filters")]
    public static string GetFavouriteFilters(
        [McpParameter("Comma-separated fields to expand", false)] string? expand = null)
    {
        var endpoint = "filter/favourite";
        
        if (!string.IsNullOrEmpty(expand))
        {
            endpoint += $"?expand={expand}";
        }

        return JiraClient.GetStringAsync(endpoint).GetAwaiter().GetResult();
    }

    [McpTool("jira_add_filter_to_favourites", "Adds a filter to favourites")]
    public static string AddFilterToFavourites(
        [McpParameter("Filter ID")] string filterId)
    {
        return JiraClient.PutStringAsync($"filter/{filterId}/favourite", null).GetAwaiter().GetResult();
    }

    [McpTool("jira_remove_filter_from_favourites", "Removes a filter from favourites")]
    public static string RemoveFilterFromFavourites(
        [McpParameter("Filter ID")] string filterId)
    {
        return JiraClient.PutStringAsync($"filter/{filterId}/favourite", null).GetAwaiter().GetResult();
    }
}


