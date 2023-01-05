namespace CanvasModel.Courses;
public record TermModel
(
  [property: JsonPropertyName("id")] ulong Id,
  [property: JsonPropertyName("name")] string Name,
  [property: JsonPropertyName("start_at")] DateTime? StartAt,
  [property: JsonPropertyName("end_at")] DateTime? EndAt
);
