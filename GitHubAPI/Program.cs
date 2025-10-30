using DotNetEnv;
using GitHubAPI.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitHubAPI
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            string envPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\.env");
            Env.Load(envPath);

            string? repo = Environment.GetEnvironmentVariable("GITHUB_REPO");
            string? token = Environment.GetEnvironmentVariable("GITHUB_PAT");
            string? ytToken = Environment.GetEnvironmentVariable("YOUTRACK_TOKEN");
            string? ytUrl = Environment.GetEnvironmentVariable("YOUTRACK_URL");
            string? ytProject = Environment.GetEnvironmentVariable("YOUTRACK_PROJECT");
            string? ytProjectReadable = Environment.GetEnvironmentVariable("YOUTRACK_PROJECT_READABLE");

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);

            using HttpClient ytClient = new HttpClient();
            ytClient.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");
            ytClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ytToken);

            ////////////////////////////////////////////

            string url = $"{ytUrl}/api/issues?fields=idReadable,summary,description,customFields(name,value(name))&query=project:{ytProjectReadable}";
            var ytResponse = await ytClient.GetAsync(url);
            ytResponse.EnsureSuccessStatusCode();


            string ytJson = await ytResponse.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(ytJson);
string prettyJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions
{
    WriteIndented = true
});

Console.WriteLine(prettyJson);

            var rawIssues = JsonSerializer.Deserialize<List<IssueWithRawFields>>(ytJson);
            var typedIssues = new List<IssueWithTypedFields>();

            foreach (var raw in rawIssues)
            {
                var stateField = raw.CustomFields.FirstOrDefault(f => f.Name == "State")?.JsonElement;
                FieldValue typedState = null;

                if (stateField.HasValue && stateField.Value.ValueKind == JsonValueKind.Object)
                {
                    typedState = JsonSerializer.Deserialize<FieldValue>(stateField.Value.GetRawText());
                }

                typedIssues.Add(new IssueWithTypedFields
                {
                    IdReadable = raw.IdReadable,
                    Summary = raw.Summary,
                    Description = raw.Description,
                    CustomFields = new[]
                    {
                    new CustomTypedField
                    {
                        Name = "State",
                        Value = typedState,
                        Type = "SingleEnumIssueCustomField"
                    }
                    }
                });
            }

            var gtResponse = await client.GetAsync($"https://api.github.com/repos/{repo}/issues?state=closed");
            gtResponse.EnsureSuccessStatusCode();

            string gtJson = await gtResponse.Content.ReadAsStringAsync();
            var gtIssues = JsonSerializer.Deserialize<List<GitHubIssue>>(gtJson);

            foreach(GitHubIssue gIssue in gtIssues)
            {
                var typedIssue = typedIssues.FirstOrDefault(i =>
                {
                    var ghField = i.CustomFields.FirstOrDefault(f => f.Name == "GitHub Number");
                    return ghField != null && ghField.Value.Equals(gIssue.number.ToString());
                });

                if (typedIssue == null)
                    continue;//should be to add it later if it cannot find it

                var stateField = typedIssue.CustomFields
                              .FirstOrDefault(f => f.Name == "State");

                string? ytState = null;

                if (stateField != null)
                {
                    if (stateField.Value is FieldValue fv)
                        ytState = fv.Name;
                    else
                        ytState = stateField.Value?.ToString();
                }

                if (typedIssue.Summary != gIssue.title || typedIssue.Description != gIssue.body || ytState != gIssue.state)
                {
                    Console.WriteLine("This one should be updated\n\n");

                    //will update tomorrow
                }

            }

            /*List<YouTrackTask> tasks = gtIssues.Select(i => new YouTrackTask
            {
               Id = i.number.ToString(),
               Title = i.title,
               Status = i.state,
               Description = i.body,
               Number = i.number.ToString()
            }).ToList();

            foreach (var task in tasks)
            {
                var issueData = new IssueWithTypedFields
                {
                    Project = new ProjectRef { Id = ytProject },
                    Summary = task.Title,
                    Description = task.Description,
                    CustomFields = new[]
                    {
                        new CustomTypedField
                        {
                            Name = "State",
                            Value = new FieldValue { Name = MapGitHubStateToYouTrack(task.Status), Type = "StateBundleElement" }
                        },
                        new CustomTypedField
                        {
                            Name = "GitHub Number",
                            Value = task.Number,
                            Type = "SimpleIssueCustomField"
                        }
                    }
                };

                var json = JsonSerializer.Serialize(issueData);
                Console.WriteLine(json);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                ytResponse = await ytClient.PostAsync($"{ytUrl}/api/issues?fields=idReadable,summary", content);

                if (ytResponse.IsSuccessStatusCode)
                {
                    string created = await ytResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Created issue: {created}");
                }
                else
                {
                    string error = await ytResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to create issue '{task.Title}': {ytResponse.StatusCode}");
                    Console.WriteLine(error);
                }
            }

            string MapGitHubStateToYouTrack(string ghState)
            {
                return ghState.ToLower() switch
                {
                    "open" => "Open",
                    "closed" => "Fixed",
                    _ => "Open"
                };
            }*/
        }
    }
}
