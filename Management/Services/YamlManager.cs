using LocalModels;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class YamlManager
{
  public string CourseToYaml(LocalCourse course)
  {
    var serializer = new SerializerBuilder().DisableAliases().Build();

    var yaml = serializer.Serialize(course);

    return yaml;
  }

  public LocalCourse ParseCourse(string rawCourse)
  {
    var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    var course = deserializer.Deserialize<LocalCourse>(rawCourse);
    return course;
  }

  public async Task SaveCourseAsync(LocalCourse course)
  {
    var courseString = CourseToYaml(course);

    var courseDirectory = $"../storage/{course.Settings.Name}";
    if (!Directory.Exists(courseDirectory))
      Directory.CreateDirectory(courseDirectory);

    await SaveModules(course);

    await File.WriteAllTextAsync($"../storage/{course.Settings.Name}.yml", courseString);
  }

  public async Task SaveModules(LocalCourse course)
  {
    var courseDirectory = $"../storage/{course.Settings.Name}";

    await SaveSettings(course, courseDirectory);
    foreach (var module in course.Modules)
    {
      var moduleDirectory = courseDirectory + "/" + module.Name;
      if (!Directory.Exists(moduleDirectory))
        Directory.CreateDirectory(moduleDirectory);

      await SaveQuizzes(course, module);
      await SaveAssignments(course, module);
    }

  }

  public async Task SaveSettings(LocalCourse course, string courseDirectory)
  {
    var settingsFilePath = courseDirectory + "/settings.yml"; ;
    var settingsYaml = course.Settings.ToYaml();
    await File.WriteAllTextAsync(settingsFilePath, settingsYaml);
  }

  public async Task SaveQuizzes(LocalCourse course, LocalModule module)
  {
    var quizzesDirectory = $"../storage/{course.Settings.Name}/{module.Name}/quizzes";
    if (!Directory.Exists(quizzesDirectory))
      Directory.CreateDirectory(quizzesDirectory);

    foreach (var quiz in module.Quizzes)
    {
      var filePath = quizzesDirectory + "/" + quiz.Name + ".yml"; ;
      var quizYaml = quiz.ToYaml();
      await File.WriteAllTextAsync(filePath, quizYaml);
    }
  }

  public async Task SaveAssignments(LocalCourse course, LocalModule module)
  {
    var assignmentsDirectory = $"../storage/{course.Settings.Name}/{module.Name}/assignments";
    if (!Directory.Exists(assignmentsDirectory))
      Directory.CreateDirectory(assignmentsDirectory);

    foreach (var assignment in module.Assignments)
    {
      var assignmentYaml = assignment.ToYaml();
      var assignmentMarkdown =
        "```yaml" + Environment.NewLine
        + assignmentYaml
        + "```" + Environment.NewLine
        + "<!-- assignment markdown below -->" + Environment.NewLine
        + assignment.Description;

      var filePath = assignmentsDirectory + "/" + assignment.Name + ".md";
      await File.WriteAllTextAsync(filePath, assignmentMarkdown);
    }
  }

  public async Task<IEnumerable<LocalCourse>> LoadSavedCourses()
  {
    string path = "../storage/";
    if (!Directory.Exists(path))
      throw new Exception("storage folder not found");

    var fileNames = Directory.GetFiles(path);

    var courses = await Task.WhenAll(
      fileNames
        .Where(name => name.EndsWith(".yml"))
        .Select(async n => ParseCourse(await File.ReadAllTextAsync($"../storage/{n}")))
    );
    return courses;
  }
}
