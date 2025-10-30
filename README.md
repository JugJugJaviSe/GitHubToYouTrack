# GitHub → YouTrack Synchronizer

This C# console application synchronizes GitHub issues with YouTrack tasks. It:

Imports all GitHub issues into YouTrack if they don’t exist.

Continuously checks for changes in GitHub issues (title, description, state) and updates YouTrack accordingly.

## Requirements

.NET 7.0 (or compatible) installed

A GitHub repository with issues

Access to a YouTrack project with API permissions

.env file with configuration (see below)

## Setup / Configuration

Create a .env file in your project root (where the .csproj is, or adjust the path in Program.cs).

Add the following variables:

GITHUB_REPO=YourGitHubUsername/YourRepoName
GITHUB_PAT=your_github_personal_access_token
YOUTRACK_TOKEN=your_youtrack_permanent_token
YOUTRACK_URL=https://yourcompany.youtrack.cloud
YOUTRACK_PROJECT=your_youtrack_project_id
YOUTRACK_PROJECT_READABLE=you_youtrack_project_readable_name

GITHUB_REPO – GitHub repository in the format username/repo.

GITHUB_PAT – GitHub Personal Access Token with repo permissions - at least read access to issues (GitHub: your profile picture -> Settings -> Developer Settings -> Personal access tokens -> Tokens (classic) -> Generate new token -> Generate new token (classic).

YOUTRACK_TOKEN – YouTrack permanent token with access to your project (YouTrack: admin (your profile) -> Profile -> Account Security -> New token).

YOUTRACK_URL – Your YouTrack instance URL.

YOUTRACK_PROJECT – YouTrack project ID (not the same as readable one). I got it through the POSTMAN (GET https://jugov.youtrack.cloud/api/admin/projects?fields=id,name,shortName&query=CLI, Bearer Token, Accept application/json (I couldn't find a better way to do it).

YOUTRACK_PROJECT_READABLE – go to the YouTrack project on web, look at the URL (https://jugov.youtrack.cloud/projects/CLI, CLI is the readable id in this example)

## How to Run

Open a terminal in the project folder (or just press the f5 while in the VS).

Restore dependencies, build the project and run the program:

dotnet restore
dotnet build
dotnet run

The program will:

Add any missing GitHub issues to YouTrack.

Continuously sync updates every 30 seconds (adjustable in Program.cs).

## Notes

The program logs each sync iteration to the console.

If no changes are detected, it prints: No changes in this iteration!

Errors are caught and displayed; the program waits 30 seconds before retrying.

New GitHub issues created while the program is running will automatically be added to YouTrack in the next iteration.
