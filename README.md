# Jira MCP Server

A comprehensive Model Context Protocol (MCP) server for Jira that provides 60+ tools for interacting with both **Jira Cloud** and **Jira Data Center/Server** instances.

Built with [Keerukee.MCPServer.Stdio](https://www.nuget.org/packages/Keerukee.MCPServer.Stdio) - a lightweight .NET library for building MCP servers.

## Features

- ✅ **Full Jira REST API Coverage** - Issues, Projects, Users, Comments, Attachments, Worklogs, and more
- ✅ **Agile/Scrum Support** - Boards, Sprints, Epics, Backlog management
- ✅ **Both Cloud & Data Center** - Single codebase supports both deployment types
- ✅ **Multiple Auth Methods** - API tokens, PAT, or username/password
- ✅ **Zero External Dependencies** - Uses only built-in .NET libraries

## Available Tools (60+)

| Category | Tools |
|----------|-------|
| **Issues** | `jira_get_issue`, `jira_create_issue`, `jira_update_issue`, `jira_delete_issue`, `jira_search_issues`, `jira_transition_issue`, `jira_assign_issue`, `jira_link_issues`, `jira_watch_issue`, `jira_vote_issue`, `jira_get_changelog`, `jira_add_labels`, `jira_remove_labels` |
| **Projects** | `jira_list_projects`, `jira_get_project`, `jira_create_project`, `jira_update_project`, `jira_delete_project`, `jira_get_project_components`, `jira_create_component`, `jira_get_project_versions`, `jira_create_version`, `jira_release_version` |
| **Users** | `jira_get_current_user`, `jira_get_user`, `jira_search_users`, `jira_find_users_assignable_to_projects`, `jira_get_users_from_group`, `jira_add_user_to_group`, `jira_get_user_permissions` |
| **Comments** | `jira_get_comments`, `jira_get_comment`, `jira_add_comment`, `jira_update_comment`, `jira_delete_comment` |
| **Attachments** | `jira_get_attachments`, `jira_get_attachment`, `jira_add_attachment_from_base64`, `jira_delete_attachment` |
| **Agile** | `jira_list_boards`, `jira_get_board`, `jira_list_sprints`, `jira_create_sprint`, `jira_start_sprint`, `jira_close_sprint`, `jira_get_sprint_issues`, `jira_move_issues_to_sprint`, `jira_get_backlog`, `jira_list_epics`, `jira_move_issues_to_epic` |
| **Worklogs** | `jira_get_worklogs`, `jira_add_worklog`, `jira_update_worklog`, `jira_delete_worklog`, `jira_get_time_tracking`, `jira_set_time_estimate` |
| **Filters** | `jira_list_filters`, `jira_get_filter`, `jira_create_filter`, `jira_update_filter`, `jira_delete_filter`, `jira_get_favourite_filters` |
| **Server** | `jira_get_server_info`, `jira_get_fields`, `jira_validate_jql`, `jira_check_connection`, `jira_get_configuration` |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Jira Cloud or Jira Data Center/Server instance
- API credentials (see Configuration section)

## Building the Server

```bash
# Clone or download the project
cd Jira_MCP

# Build the project
dotnet build -c Release

# The executable will be at:
# Windows: bin/Release/net10.0/Jira_MCP.exe
# Linux/Mac: bin/Release/net10.0/Jira_MCP
```

## Configuration

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `JIRA_BASE_URL` | ✅ | Your Jira instance URL |
| `JIRA_IS_CLOUD` | ✅ | `true` for Cloud, `false` for Data Center |
| `JIRA_EMAIL` | Cloud only | Email for Atlassian account |
| `JIRA_API_TOKEN` | Cloud only | API token from Atlassian |
| `JIRA_PAT` | DC option | Personal Access Token |
| `JIRA_USERNAME` | DC option | Username for basic auth |
| `JIRA_PASSWORD` | DC option | Password for basic auth |
| `JIRA_API_VERSION` | Optional | API version (`2` or `3`) |

### Jira Cloud Setup

1. Go to [Atlassian API Tokens](https://id.atlassian.com/manage-profile/security/api-tokens)
2. Click "Create API token"
3. Copy the generated token

```bash
# Environment variables for Jira Cloud
JIRA_BASE_URL=https://yourcompany.atlassian.net
JIRA_IS_CLOUD=true
JIRA_EMAIL=your-email@example.com
JIRA_API_TOKEN=your-api-token-here
```

### Jira Data Center Setup

**Option A: Personal Access Token (Recommended)**
1. Go to your Jira profile → Personal Access Tokens
2. Create a new token

```bash
JIRA_BASE_URL=https://jira.yourcompany.com
JIRA_IS_CLOUD=false
JIRA_PAT=your-personal-access-token
```

**Option B: Username/Password**
```bash
JIRA_BASE_URL=https://jira.yourcompany.com
JIRA_IS_CLOUD=false
JIRA_USERNAME=your-username
JIRA_PASSWORD=your-password
```

---

## MCP Client Integration

### GitHub Copilot in Visual Studio

1. Open Visual Studio 2022 (17.9+)
2. Go to **Tools** → **Options** → **GitHub** → **Copilot** → **MCP Servers**
3. Add a new server configuration:

```json
{
  "mcpServers": {
    "jira": {
      "command": "path/to/Jira_MCP.exe",
      "env": {
        "JIRA_BASE_URL": "https://yourcompany.atlassian.net",
        "JIRA_IS_CLOUD": "true",
        "JIRA_EMAIL": "your-email@example.com",
        "JIRA_API_TOKEN": "your-api-token"
      }
    }
  }
}
```

Or using `dotnet run`:
```json
{
  "mcpServers": {
    "jira": {
      "command": "dotnet",
      "args": ["run", "--project", "C:/path/to/Jira_MCP.csproj"],
      "env": {
        "JIRA_BASE_URL": "https://yourcompany.atlassian.net",
        "JIRA_IS_CLOUD": "true",
        "JIRA_EMAIL": "your-email@example.com",
        "JIRA_API_TOKEN": "your-api-token"
      }
    }
  }
}
```

---

### GitHub Copilot in VS Code

1. Install the [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) extension
2. Open VS Code Settings (JSON) or create/edit `.vscode/mcp.json` in your workspace:

```json
{
  "mcpServers": {
    "jira": {
      "command": "path/to/Jira_MCP.exe",
      "env": {
        "JIRA_BASE_URL": "https://yourcompany.atlassian.net",
        "JIRA_IS_CLOUD": "true",
        "JIRA_EMAIL": "your-email@example.com",
        "JIRA_API_TOKEN": "your-api-token"
      }
    }
  }
}
```

For Data Center:
```json
{
  "mcpServers": {
    "jira": {
      "command": "path/to/Jira_MCP.exe",
      "env": {
        "JIRA_BASE_URL": "https://jira.yourcompany.com",
        "JIRA_IS_CLOUD": "false",
        "JIRA_PAT": "your-personal-access-token"
      }
    }
  }
}
```

---

### WindSurf (Codeium)

1. Open WindSurf
2. Go to **Settings** → **MCP Servers** (or edit `~/.codeium/windsurf/mcp_config.json`)
3. Add the Jira MCP server:

```json
{
  "mcpServers": {
    "jira": {
      "command": "path/to/Jira_MCP.exe",
      "env": {
        "JIRA_BASE_URL": "https://yourcompany.atlassian.net",
        "JIRA_IS_CLOUD": "true",
        "JIRA_EMAIL": "your-email@example.com",
        "JIRA_API_TOKEN": "your-api-token"
      }
    }
  }
}
```

**macOS/Linux:**
```json
{
  "mcpServers": {
    "jira": {
      "command": "dotnet",
      "args": ["/path/to/Jira_MCP.dll"],
      "env": {
        "JIRA_BASE_URL": "https://yourcompany.atlassian.net",
        "JIRA_IS_CLOUD": "true",
        "JIRA_EMAIL": "your-email@example.com",
        "JIRA_API_TOKEN": "your-api-token"
      }
    }
  }
}
```

---

### Anthropic Claude Desktop

Edit the Claude Desktop configuration file:

- **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "jira": {
      "command": "C:/path/to/Jira_MCP.exe",
      "env": {
        "JIRA_BASE_URL": "https://yourcompany.atlassian.net",
        "JIRA_IS_CLOUD": "true",
        "JIRA_EMAIL": "your-email@example.com",
        "JIRA_API_TOKEN": "your-api-token"
      }
    }
  }
}
```

---

### Cursor

1. Open Cursor Settings
2. Navigate to **Features** → **MCP Servers**
3. Add configuration:

```json
{
  "mcpServers": {
    "jira": {
      "command": "path/to/Jira_MCP.exe",
      "env": {
        "JIRA_BASE_URL": "https://yourcompany.atlassian.net",
        "JIRA_IS_CLOUD": "true",
        "JIRA_EMAIL": "your-email@example.com",
        "JIRA_API_TOKEN": "your-api-token"
      }
    }
  }
}
```

---

### Antigravity

1. Open Antigravity settings
2. Go to **Integrations** → **MCP Servers**
3. Add a new MCP server:

```json
{
  "mcpServers": {
    "jira": {
      "command": "path/to/Jira_MCP.exe",
      "args": [],
      "env": {
        "JIRA_BASE_URL": "https://yourcompany.atlassian.net",
        "JIRA_IS_CLOUD": "true",
        "JIRA_EMAIL": "your-email@example.com",
        "JIRA_API_TOKEN": "your-api-token"
      }
    }
  }
}
```

Or for Jira Data Center with PAT:
```json
{
  "mcpServers": {
    "jira": {
      "command": "path/to/Jira_MCP.exe",
      "env": {
        "JIRA_BASE_URL": "https://jira.yourcompany.com",
        "JIRA_IS_CLOUD": "false",
        "JIRA_PAT": "your-personal-access-token"
      }
    }
  }
}
```

---

## Usage Examples

Once connected, you can ask your AI assistant to perform Jira operations:

### Issue Management
```
"Create a bug in project MYPROJ titled 'Login button not working' with high priority"
"Search for all open issues assigned to me"
"Transition MYPROJ-123 to Done"
"Add a comment to MYPROJ-456 saying 'Fixed in latest build'"
```

### Sprint Management
```
"List all active sprints for board 42"
"Create a new sprint called 'Sprint 15' for board 42"
"Move issues MYPROJ-100, MYPROJ-101 to sprint 15"
"Close sprint 14"
```

### Time Tracking
```
"Log 2 hours of work on MYPROJ-123 with comment 'Code review'"
"What's the remaining estimate for MYPROJ-456?"
"Set the original estimate for MYPROJ-789 to 3 days"
```

### Project Information
```
"List all projects I have access to"
"Get all components for project MYPROJ"
"Create a new version '2.0.0' in project MYPROJ"
```

---

## Project Structure

```
Jira_MCP/
├── Program.cs                 # MCP server entry point
├── Jira_MCP.csproj           # Project file
├── README.md                  # This file
├── Models/
│   └── JiraModels.cs         # Jira API data models
├── Services/
│   └── JiraClient.cs         # HTTP client for Jira API
└── Tools/
    ├── IssueTools.cs         # Issue operations
    ├── ProjectTools.cs       # Project operations
    ├── UserTools.cs          # User operations
    ├── CommentTools.cs       # Comment operations
    ├── AttachmentTools.cs    # Attachment operations
    ├── AgileTools.cs         # Agile/Scrum operations
    ├── WorklogTools.cs       # Time tracking operations
    ├── FilterTools.cs        # Filter operations
    └── ServerTools.cs        # Server info operations
```

---

## Troubleshooting

### Connection Issues

Use the `jira_check_connection` tool to verify your setup:
```
"Check my Jira connection"
```

### Authentication Errors

- **Cloud:** Ensure `JIRA_IS_CLOUD=true` and both `JIRA_EMAIL` and `JIRA_API_TOKEN` are set
- **Data Center:** Ensure `JIRA_IS_CLOUD=false` and either `JIRA_PAT` or both `JIRA_USERNAME`/`JIRA_PASSWORD` are set

### Permission Errors

Ensure your Jira user has the necessary permissions for the operations you're trying to perform.

---

## License

MIT License

## Author

Built with [Keerukee.MCPServer.Stdio](https://www.nuget.org/packages/Keerukee.MCPServer.Stdio)
