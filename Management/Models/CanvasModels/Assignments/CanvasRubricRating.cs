namespace CanvasModel.Assignments;

public record CanvasRubricRating
(
  [property: JsonPropertyName("points")]
  double Points,

  [property: JsonPropertyName("id")]
  string Id,

  [property: JsonPropertyName("description")]
  string Description,

  [property: JsonPropertyName("long_description")]
  string LongDescription
);