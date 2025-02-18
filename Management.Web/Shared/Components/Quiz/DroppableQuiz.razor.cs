using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;

namespace Management.Web.Shared.Components.Quiz;

public class DroppableQuiz : ComponentBase
{
  [Inject]
  protected CoursePlanner planner { get; set; } = default!;

  [Parameter, EditorRequired]
  public LocalQuiz Quiz { get; set; } = default!;


  internal void dropCallback(DateTime? dropDate, LocalModule? dropModule)
  {
    if (dropDate != null)
    {
      dropOnDate(dropDate ?? throw new Exception("drop date for quiz is null"));
    }
    else if (dropModule != null)
    {
      dropOnModule(dropModule);
    }
  }

  private void dropOnDate(DateTime dropDate)
  {
    if (planner.LocalCourse == null)
      return;
    var currentModule =
      planner.LocalCourse.Modules.First(m => m.Quizzes.Select(q => q.Name + q.Description).Contains(Quiz.Name + Quiz.Description))
      ?? throw new Exception("in quiz callback, could not find module");

    var defaultDueTimeDate = new DateTime(
      year: dropDate.Year,
      month: dropDate.Month,
      day: dropDate.Day,
      hour: planner.LocalCourse.Settings.DefaultDueTime.Hour,
      minute: planner.LocalCourse.Settings.DefaultDueTime.Minute,
      second: 0
    );

    var NewQuizList = currentModule.Quizzes
      .Select(q => 
        q.Name + q.Description != Quiz.Name + Quiz.Description 
          ? q : 
          q with { 
            DueAt = defaultDueTimeDate,
            LockAt = q.LockAt > defaultDueTimeDate ? q.LockAt : defaultDueTimeDate
          }
      )
      .ToArray();

    var updatedModule = currentModule with { Quizzes = NewQuizList };
    var updatedModules = planner.LocalCourse.Modules
      .Select(m => m.Name == updatedModule.Name ? updatedModule : m)
      .ToArray();

    planner.LocalCourse = planner.LocalCourse with { Modules = updatedModules };
  }

  private void dropOnModule(LocalModule dropModule)
  {
    if (planner.LocalCourse == null)
      return;
    var newModules = planner.LocalCourse.Modules
      .Select(
        m =>
          m.Name != dropModule.Name
            ? m with
            {
              Quizzes = m.Quizzes.Where(q => q.Name + q.Description != Quiz.Name + Quiz.Description).DistinctBy(q => q.Name + q.Description)
            }
            : m with
            {
              Quizzes = m.Quizzes.Append(Quiz).DistinctBy(q => q.Name + q.Description)
            }
      )
      .ToArray();

    var newCourse = planner.LocalCourse with { Modules = newModules };
    planner.LocalCourse = newCourse;
  }
}
