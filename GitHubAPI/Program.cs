using DotNetEnv;
using GitHubAPI.Helpers;
using GitHubAPI.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

            var addYouTrackIssue = new AddYouTrackIssue();
            var updateYouTrackIssue = new UpdateYouTrackIssue();

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);

            using HttpClient ytClient = new HttpClient();
            ytClient.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");
            ytClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ytToken);

            string url = $"{ytUrl}/api/issues?fields=idReadable,summary,description,customFields(name,value(name))&query=project:{ytProjectReadable}";

            while (true)
            {
                try
                {
                    var ytResponse = await ytClient.GetAsync(url);
                    ytResponse.EnsureSuccessStatusCode();


                    string ytJson = await ytResponse.Content.ReadAsStringAsync();
                    var rawIssues = JsonSerializer.Deserialize<List<IssueWithRawFields>>(ytJson);
                    var typedIssues = new List<IssueWithTypedFields>();

                    foreach (var raw in rawIssues)
                    {
                        var typedFields = raw.CustomFields.Select(f =>
                        {
                            object? value = null;

                            if (f.JsonElement is JsonElement elem)
                            {
                                switch (elem.ValueKind)
                                {
                                    case JsonValueKind.Object:
                                        value = JsonSerializer.Deserialize<FieldValue>(elem.GetRawText());
                                        break;
                                    case JsonValueKind.String:
                                        value = elem.GetString();
                                        break;
                                    case JsonValueKind.Array:
                                        value = JsonSerializer.Deserialize<List<FieldValue>>(elem.GetRawText());
                                        break;
                                }
                            }

                            return new CustomTypedField
                            {
                                Name = f.Name,
                                Type = f.Type,
                                Value = value
                            };
                        }).ToArray();

                        typedIssues.Add(new IssueWithTypedFields
                        {
                            IdReadable = raw.IdReadable,
                            Summary = raw.Summary,
                            Description = raw.Description,
                            CustomFields = typedFields
                        });
                    }

                    var gtResponse = await client.GetAsync($"https://api.github.com/repos/{repo}/issues?state=all");
                    gtResponse.EnsureSuccessStatusCode();

                    string gtJson = await gtResponse.Content.ReadAsStringAsync();
                    var gtIssues = JsonSerializer.Deserialize<List<GitHubIssue>>(gtJson);

                    bool change = false;

                    foreach (GitHubIssue gIssue in gtIssues)
                    {
                        var typedIssue = typedIssues.FirstOrDefault(i =>
                        {
                            var ghField = i.CustomFields.FirstOrDefault(f => f.Name == "GitHub Number");
                            return ghField != null && ghField.Value.Equals(gIssue.number.ToString());
                        });

                        if (typedIssue == null) //adding a new one if it isn't in the typedIssues(TouTrack) but it is in the gtIssues (GitHub)
                        {
                            await addYouTrackIssue.AddIssue(ytProject, gIssue, ytClient, ytUrl);
                            change = true;
                            continue;
                        }

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

                        //check if it needs to be updated, and then updating the issue if the YouTrack one is different from the Github one

                        string gState = addYouTrackIssue.MapGitHubStateToYouTrack(gIssue.state);

                        if (typedIssue.Summary != gIssue.title || typedIssue.Description != gIssue.body || ytState != gState)
                        {
                            await updateYouTrackIssue.UpdateIssue(gIssue, ytClient, ytUrl, typedIssue);
                            change = true;
                        }
                    }
                    if (!change)
                    {
                        Console.WriteLine("No changes in this iteration!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during sync: {ex.Message}");
                }
                Console.WriteLine("Waiting 30 seconds before next sync...");
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
    }
}
