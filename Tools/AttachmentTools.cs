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
    public static async Task<string> GetAttachments(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey)
    {
        var issue = await JiraClient.GetAsync<JiraIssue>($"issue/{issueKey}?fields=attachment");
        return JiraClient.ToJson(issue?.Fields?.Attachment);
    }

    [McpTool("jira_get_attachment", "Gets metadata for a specific attachment")]
    public static async Task<string> GetAttachment(
        [McpParameter("Attachment ID")] string attachmentId)
    {
        var result = await JiraClient.GetAsync<JiraAttachment>($"attachment/{attachmentId}");
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_add_attachment_from_base64", "Adds an attachment to an issue from base64 encoded content")]
    public static async Task<string> AddAttachmentFromBase64(
        [McpParameter("Issue key (e.g., PROJ-123)")] string issueKey,
        [McpParameter("Base64 encoded file content")] string base64Content,
        [McpParameter("File name with extension (e.g., document.pdf)")] string fileName)
    {
        var fileContent = Convert.FromBase64String(base64Content);
        var result = await JiraClient.UploadFileAsync<List<JiraAttachment>>($"issue/{issueKey}/attachments", fileContent, fileName);
        return JiraClient.ToJson(result);
    }

    [McpTool("jira_delete_attachment", "Deletes an attachment")]
    public static async Task<string> DeleteAttachment(
        [McpParameter("Attachment ID")] string attachmentId)
    {
        await JiraClient.DeleteAsync($"attachment/{attachmentId}");
        return $"Attachment {attachmentId} deleted";
    }

    [McpTool("jira_get_attachment_metadata", "Gets global attachment settings")]
    public static async Task<string> GetAttachmentMeta()
    {
        var result = await JiraClient.GetAsync<object>("attachment/meta");
        return JiraClient.ToJson(result);
    }
}
