using System.Collections;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace LocalModels;

public record LocalAssignment
{
  private string _name = "";
  public string Name
  {
    get => _name;
    init
    {
      if (value.Contains(':'))
        throw new AssignmentMarkdownParseException("Name cannot contain a ':' character, it breaks windows filesystems " + value);
      _name = value;
    }
  }
  public string Description { get; init; } = "";
  // public bool LockAtDueDate { get; init; }
  public DateTime? LockAt { get; init; }
  public DateTime DueAt { get; init; }
  public string? LocalAssignmentGroupName { get; init; }
  public IEnumerable<string> SubmissionTypes { get; init; } = Array.Empty<string>();
  public IEnumerable<RubricItem> Rubric { get; init; } = Array.Empty<RubricItem>();
  public int PointsPossible => Rubric.Sum(r => r.IsExtraCredit ? 0 : r.Points);
  public string GetRubricHtml()
  {
    var output = "<h2>Rubric</h2>";
    var lineStrings = Rubric.Select(
      item => $"- {item.Points}pts: {item.Label} <br/>"
    );
    output += string.Join("\n", lineStrings);
    return output;
  }

  public string GetDescriptionHtml()
  {
    var rubricHtml = GetRubricHtml();

    return Markdig.Markdown.ToHtml(Description) + "<hr>" + rubricHtml;
  }

  public ulong? GetCanvasAssignmentGroupId(IEnumerable<LocalAssignmentGroup> assignmentGroups) =>
    assignmentGroups
      .FirstOrDefault(g => g.Name == LocalAssignmentGroupName)?
      .CanvasId;


  public string ToYaml()
  {
    var serializer = new SerializerBuilder().DisableAliases().Build();
    var yaml = serializer.Serialize(this);
    return yaml;
  }

  public static LocalAssignment ParseMarkdown(string input)
  {
    var settingsString = input.Split("---")[0];
    var (name, localAssignmentGroupName, submissionTypes, dueAt, lockAt) = parseSettings(settingsString);

    var description = input.Split("---\n")[1].Split("## Rubric")[0];

    var rubricString = input.Split("## Rubric\n")[1];
    var rubric = ParseRubricMarkdown(rubricString);
    return new LocalAssignment()
    {
      Name = name.Trim(),
      LocalAssignmentGroupName = localAssignmentGroupName.Trim(),
      SubmissionTypes = submissionTypes,
      DueAt = dueAt,
      LockAt = lockAt,
      Rubric = rubric,
      Description = description.Trim()
    };
  }

  private static (string name, string assignmentGroupName, List<string> submissionTypes, DateTime dueAt, DateTime? lockAt) parseSettings(string input)
  {
    var name = extractLabelValue(input, "Name");
    var rawLockAt = extractLabelValue(input, "LockAt");
    var rawDueAt = extractLabelValue(input, "DueAt");
    var localAssignmentGroupName = extractLabelValue(input, "AssignmentGroupName");
    var submissionTypes = parseSubmissionTypes(input);

    DateTime? lockAt = DateTime.TryParse(rawLockAt, out DateTime parsedLockAt)
      ? parsedLockAt
      : null;
    var dueAt = DateTime.TryParse(rawDueAt, out DateTime parsedDueAt)
      ? parsedDueAt
      : throw new QuizMarkdownParseException($"Error with DueAt: {rawDueAt}");

    return (name, localAssignmentGroupName, submissionTypes, dueAt, lockAt);


  }

  private static List<string> parseSubmissionTypes(string input)
  {
    input = input.Replace("\r\n", "\n");
    List<string> submissionTypes = new List<string>();

    // Define a regular expression pattern to match the bulleted list items
    string startOfTypePattern = @"- (.+)";
    Regex regex = new Regex(startOfTypePattern);

    var words = input.Split("SubmissionTypes:");
    var inputAfterSubmissionTypes = words[1];

    string[] lines = inputAfterSubmissionTypes.Split("\n", StringSplitOptions.RemoveEmptyEntries);

    foreach (string line in lines)
    {
      string trimmedLine = line.Trim();
      Match match = regex.Match(trimmedLine);

      if (!match.Success)
        break;

      string type = match.Groups[1].Value.Trim();
      submissionTypes.Add(type);
    }

    return submissionTypes;
  }

  static string extractLabelValue(string input, string label)
  {
    string pattern = $@"{label}: (.*?)\n";
    Match match = Regex.Match(input, pattern);

    if (match.Success)
    {
      return match.Groups[1].Value;
    }

    return string.Empty;
  }

  public string ToMarkdown()
  {
    var settingsMarkdown = settingsToMarkdown();
    var rubricMarkdown = RubricToMarkdown();
    var assignmentMarkdown =
      settingsMarkdown + "\n"
      + "---\n\n"
      + Description + "\n\n"
      + "## Rubric\n\n"
      + rubricMarkdown;

    return assignmentMarkdown;
  }

  public string RubricToMarkdown()
  {
    var builder = new StringBuilder();
    foreach (var item in Rubric)
    {
      var pointLabel = item.Points > 1 ? "pts" : "pt";
      builder.Append($"- {item.Points}{pointLabel}: {item.Label}" + "\n");
    }
    return builder.ToString();
  }

  private string settingsToMarkdown()
  {
    var builder = new StringBuilder();
    builder.Append($"Name: {Name}" + "\n");
    builder.Append($"LockAt: {LockAt}" + "\n");
    builder.Append($"DueAt: {DueAt}" + "\n");
    builder.Append($"AssignmentGroupName: {LocalAssignmentGroupName}" + "\n");
    builder.Append($"SubmissionTypes:" + "\n");
    foreach (var submissionType in SubmissionTypes)
    {
      builder.Append($"- {submissionType}" + "\n");
    }
    return builder.ToString();
  }

  public static IEnumerable<RubricItem> ParseRubricMarkdown(string rawMarkdown)
  {
    if (rawMarkdown.Trim() == string.Empty)
      return [];
    var lines = rawMarkdown.Trim().Split("\n");
    var items = lines.Select(parseIndividualRubricItemMarkdown).ToArray();
    return items;
  }

  private static RubricItem parseIndividualRubricItemMarkdown(string rawMarkdown)
  {
    var pointsPattern = @"\s*-\s*(\d+)\s*pt(s)?:";
    var match = Regex.Match(rawMarkdown, pointsPattern);
    if (!match.Success)
      throw new RubricMarkdownParseException($"points not found: {rawMarkdown}");

    var points = int.Parse(match.Groups[1].Value);

    var label = string.Join(": ", rawMarkdown.Split(": ").Skip(1));

    return new RubricItem()
    {
      Points = points,
      Label = label
    };
  }
}

public class RubricMarkdownParseException : Exception
{
  public RubricMarkdownParseException(string message) : base(message) { }
}
public class AssignmentMarkdownParseException : Exception
{
  public AssignmentMarkdownParseException(string message) : base(message) { }
}