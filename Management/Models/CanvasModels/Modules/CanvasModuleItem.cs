namespace CanvasModel.Modules;

public record CanvasModuleItem(
  [property: JsonPropertyName("id")] ulong Id,
  [property: JsonPropertyName("module_id")] ulong ModuleId,
  [property: JsonPropertyName("position")] int Position,
  [property: JsonPropertyName("title")] string Title,
  [property: JsonPropertyName("indent")] uint? Indent,
  [property: JsonPropertyName("type")] string Type,
  [property: JsonPropertyName("content_id")] ulong? ContentId,
  [property: JsonPropertyName("html_url")] string HtmlUrl,
  [property: JsonPropertyName("url")] string? Url,
  [property: JsonPropertyName("page_url")] string? PageUrl,
  [property: JsonPropertyName("external_url")] string? ExternalUrl,
  [property: JsonPropertyName("new_tab")] bool NewTab,
  // [OptIn]
  [property: JsonPropertyName("completion_requirement")]
    CanvasCompletionRequirement? CompletionRequirement,
  [property: JsonPropertyName("published")] bool? Published,
  [property: JsonPropertyName("content_details")] CanvasModuleItemContentDetails? ContentDetails
);

public record CanvasModuleItemContentDetails(
  [property: JsonPropertyName("due_at")] DateTime? DueAt,
  [property: JsonPropertyName("lock_at")] DateTime? LockAt,
  [property: JsonPropertyName("points_possible")] double PointsPossible,
  [property: JsonPropertyName("locked_for_user")] bool LockedForUser
);