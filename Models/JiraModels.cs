using System.Text.Json.Serialization;

namespace JiraMcp.Models;

#region Common Models

public record JiraUser(
    string? AccountId,
    string? Key,
    string? Name,
    string? EmailAddress,
    string? DisplayName,
    bool? Active,
    string? TimeZone,
    AvatarUrls? AvatarUrls
);

public record AvatarUrls(
    [property: JsonPropertyName("48x48")] string? Large,
    [property: JsonPropertyName("24x24")] string? Small,
    [property: JsonPropertyName("16x16")] string? XSmall,
    [property: JsonPropertyName("32x32")] string? Medium
);

public record JiraStatus(
    string? Id,
    string? Name,
    string? Description,
    StatusCategory? StatusCategory
);

public record StatusCategory(
    int? Id,
    string? Key,
    string? ColorName,
    string? Name
);

public record JiraPriority(
    string? Id,
    string? Name,
    string? Description,
    string? IconUrl
);

public record JiraResolution(
    string? Id,
    string? Name,
    string? Description
);

public record PagedResult<T>(
    int? StartAt,
    int? MaxResults,
    int? Total,
    List<T>? Values
);

public record SearchResult(
    string? Expand,
    int? StartAt,
    int? MaxResults,
    int? Total,
    List<JiraIssue>? Issues
);

#endregion

#region Issue Models

public record JiraIssue(
    string? Id,
    string? Key,
    string? Self,
    IssueFields? Fields,
    RenderedFields? RenderedFields,
    Changelog? Changelog
);

/// <summary>
/// Fields returned when reading a Jira issue.
/// Note: Description and Environment are object? to support both plain text (Data Center V2)
/// and Atlassian Document Format/ADF JSON (Cloud V3) in API responses.
/// </summary>
public record IssueFields(
    string? Summary,
    object? Description,
    IssueType? IssueType,
    JiraProject? Project,
    JiraStatus? Status,
    JiraPriority? Priority,
    JiraResolution? Resolution,
    JiraUser? Assignee,
    JiraUser? Reporter,
    JiraUser? Creator,
    DateTime? Created,
    DateTime? Updated,
    DateTime? ResolutionDate,
    DateTime? DueDate,
    List<string>? Labels,
    List<JiraComponent>? Components,
    List<JiraVersion>? FixVersions,
    List<JiraVersion>? AffectedVersions,
    JiraIssue? Parent,
    List<JiraIssue>? Subtasks,
    List<IssueLink>? IssueLinks,
    TimeTracking? TimeTracking,
    Watches? Watches,
    Votes? Votes,
    CommentContainer? Comment,
    AttachmentContainer? Attachment,
    WorklogContainer? Worklog,
    object? Environment
);

public record RenderedFields(
    string? Description,
    string? Comment
);

public record IssueType(
    string? Id,
    string? Name,
    string? Description,
    bool? Subtask,
    string? IconUrl
);

public record JiraComponent(
    string? Id,
    string? Name,
    string? Description,
    JiraUser? Lead
);

public record JiraVersion(
    string? Id,
    string? Name,
    string? Description,
    bool? Released,
    bool? Archived,
    DateTime? ReleaseDate
);

public record IssueLink(
    string? Id,
    IssueLinkType? Type,
    JiraIssue? InwardIssue,
    JiraIssue? OutwardIssue
);

public record IssueLinkType(
    string? Id,
    string? Name,
    string? Inward,
    string? Outward
);

public record TimeTracking(
    string? OriginalEstimate,
    string? RemainingEstimate,
    string? TimeSpent,
    int? OriginalEstimateSeconds,
    int? RemainingEstimateSeconds,
    int? TimeSpentSeconds
);

public record Watches(
    string? Self,
    int? WatchCount,
    bool? IsWatching
);

public record Votes(
    string? Self,
    int? VoteCount,
    bool? HasVoted
);

public record Changelog(
    int? StartAt,
    int? MaxResults,
    int? Total,
    List<ChangelogHistory>? Histories
);

public record ChangelogHistory(
    string? Id,
    JiraUser? Author,
    DateTime? Created,
    List<ChangelogItem>? Items
);

public record ChangelogItem(
    string? Field,
    string? FieldType,
    string? From,
    string? FromString,
    string? To,
    string? ToStringValue
);

#endregion

#region Comment Models

public record CommentContainer(
    int? StartAt,
    int? MaxResults,
    int? Total,
    List<JiraComment>? Comments
);

/// <summary>
/// Jira comment model. Body is object? to support plain text (Data Center V2) and ADF JSON (Cloud V3).
/// </summary>
public record JiraComment(
    string? Id,
    string? Self,
    JiraUser? Author,
    JiraUser? UpdateAuthor,
    object? Body,
    DateTime? Created,
    DateTime? Updated,
    CommentVisibility? Visibility
);

public record CommentVisibility(
    string? Type,
    string? Value
);

#endregion

#region Attachment Models

public record AttachmentContainer(
    List<JiraAttachment>? Attachments
);

public record JiraAttachment(
    string? Id,
    string? Self,
    string? Filename,
    JiraUser? Author,
    DateTime? Created,
    long? Size,
    string? MimeType,
    string? Content,
    string? Thumbnail
);

#endregion

#region Worklog Models

public record WorklogContainer(
    int? StartAt,
    int? MaxResults,
    int? Total,
    List<JiraWorklog>? Worklogs
);

/// <summary>
/// Jira worklog entry. Comment is object? to support plain text (Data Center V2) and ADF JSON (Cloud V3).
/// </summary>
public record JiraWorklog(
    string? Id,
    string? Self,
    JiraUser? Author,
    JiraUser? UpdateAuthor,
    object? Comment,
    DateTime? Created,
    DateTime? Updated,
    DateTime? Started,
    string? TimeSpent,
    int? TimeSpentSeconds
);

#endregion

#region Project Models

public record JiraProject(
    string? Id,
    string? Key,
    string? Name,
    string? Description,
    JiraUser? Lead,
    string? ProjectTypeKey,
    AvatarUrls? AvatarUrls,
    List<IssueType>? IssueTypes,
    List<JiraComponent>? Components,
    List<JiraVersion>? Versions,
    ProjectCategory? ProjectCategory
);

public record ProjectCategory(
    string? Id,
    string? Name,
    string? Description
);

public record CreateProjectRequest(
    string Key,
    string Name,
    string ProjectTypeKey,
    string? Description,
    string? LeadAccountId,
    string? Lead,
    string? CategoryId,
    string? AssigneeType
);

#endregion

#region Transition Models

public record TransitionsResult(
    string? Expand,
    List<JiraTransition>? Transitions
);

public record JiraTransition(
    string? Id,
    string? Name,
    JiraStatus? To,
    bool? HasScreen,
    bool? IsGlobal,
    bool? IsInitial,
    bool? IsAvailable,
    bool? IsConditional,
    List<TransitionField>? Fields
);

public record TransitionField(
    bool? Required,
    FieldSchema? Schema,
    string? Name,
    string? Key,
    List<object>? AllowedValues
);

public record FieldSchema(
    string? Type,
    string? System,
    string? Custom,
    int? CustomId
);

#endregion

#region Agile Models

public record JiraBoard(
    int? Id,
    string? Self,
    string? Name,
    string? Type,
    BoardLocation? Location
);

public record BoardLocation(
    int? ProjectId,
    string? ProjectKey,
    string? ProjectName,
    string? DisplayName,
    string? ProjectTypeKey
);

public record BoardsResult(
    int? MaxResults,
    int? StartAt,
    int? Total,
    bool? IsLast,
    List<JiraBoard>? Values
);

public record JiraSprint(
    int? Id,
    string? Self,
    string? State,
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? CompleteDate,
    int? OriginBoardId,
    string? Goal
);

public record SprintsResult(
    int? MaxResults,
    int? StartAt,
    int? Total,
    bool? IsLast,
    List<JiraSprint>? Values
);

public record SprintIssuesResult(
    int? MaxResults,
    int? StartAt,
    int? Total,
    List<JiraIssue>? Issues
);

public record JiraEpic(
    int? Id,
    string? Key,
    string? Self,
    string? Name,
    string? Summary,
    bool? Done
);

public record BacklogIssuesResult(
    int? MaxResults,
    int? StartAt,
    int? Total,
    List<JiraIssue>? Issues
);

#endregion

#region Filter Models

public record JiraFilter(
    string? Id,
    string? Self,
    string? Name,
    string? Description,
    JiraUser? Owner,
    string? Jql,
    string? ViewUrl,
    string? SearchUrl,
    bool? Favourite,
    List<SharePermission>? SharePermissions
);

public record SharePermission(
    string? Id,
    string? Type,
    JiraProject? Project,
    JiraRole? Role,
    JiraGroup? Group
);

public record JiraRole(
    string? Id,
    string? Name,
    string? Description
);

public record JiraGroup(
    string? Name,
    string? Self
);

#endregion

#region Field Models

public record JiraField(
    string? Id,
    string? Key,
    string? Name,
    bool? Custom,
    bool? Orderable,
    bool? Navigable,
    bool? Searchable,
    List<string>? ClauseNames,
    FieldSchema? Schema
);

#endregion

#region Server Info

public record ServerInfo(
    string? BaseUrl,
    string? Version,
    int[]? VersionNumbers,
    string? DeploymentType,
    DateTime? BuildDate,
    string? ServerTitle
);

#endregion

#region Request Models

public record CreateIssueRequest
{
    public CreateIssueFields? Fields { get; init; }
}

/// <summary>
/// Fields for creating a new Jira issue.
/// Description and Environment are object? to hold either plain text (Data Center V2)
/// or ADF JSON (Cloud V3). Use JiraClient.FormatTextForApi() to set these fields.
/// </summary>
public record CreateIssueFields
{
    public ProjectRef? Project { get; init; }
    public IssueTypeRef? IssueType { get; init; }
    public string? Summary { get; init; }
    public object? Description { get; init; }
    public UserRef? Assignee { get; init; }
    public UserRef? Reporter { get; init; }
    public PriorityRef? Priority { get; init; }
    public List<string>? Labels { get; init; }
    public List<ComponentRef>? Components { get; init; }
    public List<VersionRef>? FixVersions { get; init; }
    public ParentRef? Parent { get; init; }
    public string? DueDate { get; init; }
    public TimeTrackingInput? TimeTracking { get; init; }
    public object? Environment { get; init; }
}

public record UpdateIssueRequest
{
    public UpdateIssueFields? Fields { get; init; }
}

/// <summary>
/// Fields for updating an existing Jira issue.
/// Description and Environment are object? to hold either plain text (Data Center V2)
/// or ADF JSON (Cloud V3). Use JiraClient.FormatTextForApi() to set these fields.
/// </summary>
public record UpdateIssueFields
{
    public string? Summary { get; init; }
    public object? Description { get; init; }
    public UserRef? Assignee { get; init; }
    public PriorityRef? Priority { get; init; }
    public List<string>? Labels { get; init; }
    public List<ComponentRef>? Components { get; init; }
    public List<VersionRef>? FixVersions { get; init; }
    public string? DueDate { get; init; }
    public TimeTrackingInput? TimeTracking { get; init; }
    public object? Environment { get; init; }
}

public record ProjectRef(string? Id = null, string? Key = null);
public record IssueTypeRef(string? Id = null, string? Name = null);
public record UserRef(string? AccountId = null, string? Name = null);
public record PriorityRef(string? Id = null, string? Name = null);
public record ComponentRef(string? Id = null, string? Name = null);
public record VersionRef(string? Id = null, string? Name = null);
public record ParentRef(string? Key = null, string? Id = null);
public record TimeTrackingInput(string? OriginalEstimate = null, string? RemainingEstimate = null);

public record TransitionRequest(string Id, TransitionFields? Fields = null);
public record TransitionFields(ResolutionRef? Resolution = null);
public record ResolutionRef(string? Name = null, string? Id = null);

public record CreateCommentRequest(string Body, CommentVisibility? Visibility = null);
public record UpdateCommentRequest(string Body, CommentVisibility? Visibility = null);

public record AddWorklogRequest(
    string TimeSpent,
    DateTime? Started = null,
    string? Comment = null
);

public record CreateSprintRequest(
    string Name,
    int OriginBoardId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Goal = null
);

public record UpdateSprintRequest(
    string? Name = null,
    string? State = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Goal = null
);

public record MoveIssuesToSprintRequest(List<string> Issues);

public record CreateIssueResponse(string Id, string Key, string Self);

#endregion
