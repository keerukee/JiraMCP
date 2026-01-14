using MCPServer;

// Jira MCP Server - Provides comprehensive Jira tools for both Cloud and Data Center
// 
// Environment Variables Required:
// 
// Common:
//   JIRA_BASE_URL     - Your Jira URL (e.g., https://yourcompany.atlassian.net or https://jira.yourcompany.com)
//   JIRA_IS_CLOUD     - Set to "true" for Jira Cloud, "false" for Data Center/Server
//
// For Jira Cloud (JIRA_IS_CLOUD=true):
//   JIRA_EMAIL        - Email address associated with your Atlassian account
//   JIRA_API_TOKEN    - API token generated from https://id.atlassian.com/manage-profile/security/api-tokens
//
// For Jira Data Center / Server (JIRA_IS_CLOUD=false):
//   Option A - Personal Access Token (recommended):
//     JIRA_PAT        - Personal Access Token
//   Option B - Username/Password:
//     JIRA_USERNAME   - Your Jira username
//     JIRA_PASSWORD   - Your Jira password
//
// Optional:
//   JIRA_API_VERSION  - API version to use (default: "2" for Data Center, "3" for Cloud)

var server = new McpServerHost(new McpServerOptions
{
    ServerName = "Jira MCP Server",
    ServerVersion = "1.0.0"
});

server.Run();
