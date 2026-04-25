using JiraMcp.Models;
using JiraMcp.Services;
using MCPServer.Attributes;

namespace JiraMcp.Tools;

/// <summary>
/// MCP Tools for Jira Attachment operations (Cloud and Data Center)
/// </summary>
public static class AttachmentTools
{
    [McpTool("jira_get_attachments", "Gets all attachments on an issue")]
    public static string GetAttachments(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey)
    {
        var rawJson = JiraClient.GetStringAsync($"issue/{issueKey}?fields=attachment").GetAwaiter().GetResult();
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
            if (doc.RootElement.TryGetProperty("fields", out var fields) &&
                fields.TryGetProperty("attachment", out var attachment))
            {
                return attachment.GetRawText();
            }
        }
        catch { }
        return "[]";
    }

    [McpTool("jira_get_attachment", "Gets metadata for a specific attachment")]
    public static string GetAttachment(
        [McpParameter("Attachment ID")] string attachmentId)
    {
        return JiraClient.GetStringAsync($"attachment/{attachmentId}").GetAwaiter().GetResult();
    }

    [McpTool("jira_add_attachment_from_base64", "Adds an attachment to an issue from base64 encoded content")]
    public static string AddAttachmentFromBase64(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Base64 encoded file content")] string base64Content,
        [McpParameter("File name with extension (e.g., document.pdf)")] string fileName)
    {
        var fileContent = Convert.FromBase64String(base64Content);
        return JiraClient.UploadFileAsync<string>($"issue/{issueKey}/attachments", fileContent, fileName).GetAwaiter().GetResult() ?? "[]";
    }

    [McpTool("jira_delete_attachment", "Deletes an attachment")]
    public static string DeleteAttachment(
        [McpParameter("Attachment ID")] string attachmentId)
    {
        JiraClient.DeleteAsync($"attachment/{attachmentId}").GetAwaiter().GetResult();
        return $"Attachment {attachmentId} deleted";
    }

    [McpTool("jira_get_attachment_metadata", "Gets global attachment settings")]
    public static string GetAttachmentMeta()
    {
        return JiraClient.GetStringAsync("attachment/meta").GetAwaiter().GetResult();
    }
}


