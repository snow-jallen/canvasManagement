# canvasManagement


install specflow template `dotnet new install Specflow.Templates.DotNet`

view templates with `dotnet new -l`

find outdated packages `dotnet list package --outdated`


Development command: `dotnet watch --project Management.Web/`


# Razor Hack

Apparently the VSCode razor extension was compiled with a preview of dotnet 6... and only uses openssl 1.1. After installing openssl1.1 you can tell vscode to provide it with `export CLR_OPENSSL_VERSION_OVERRIDE=1.1; code ~/projects/canvasManagement`.

The issue can be tracked [here](https://github.com/dotnet/razor/issues/6241)




"Name: test assignment
LockAt: 1/1/0001 12:00:00 AM
DueAt: 1/1/0001 12:00:00 AM
AssignmentGroupName: Final Project
SubmissionTypes:
- online_upload

---
here is the description
## Rubric
- 4pts: do task 1
- 2pts: do task 2
"


"Name: test assignment
LockAt: 1/1/0001 12:00:00 AM
DueAt: 1/1/0001 12:00:00 AM
AssignmentGroupName: Final Project
SubmissionTypes:

---
