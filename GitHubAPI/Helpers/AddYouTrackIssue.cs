using GitHubAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitHubAPI.Helpers
{
    public class AddYouTrackIssue
    {
        public async Task AddIssue(string ytProject, GitHubIssue gIssue, HttpClient ytClient, string ytUrl)
        {
            var newIssue = new IssueWithTypedFields
            {
                Project = new ProjectRef { Id = ytProject },
                Summary = gIssue.title,
                Description = gIssue.body,
                CustomFields = new[]
                        {
                            new CustomTypedField
                            {
                                Name = "State",
                                Value = new FieldValue { Name = MapGitHubStateToYouTrack(gIssue.state), Type = "StateBundleElement" }
                            },
                            new CustomTypedField
                            {
                                Name = "GitHub Number",
                                Value = gIssue.number,
                                Type = "SimpleIssueCustomField"
                            }
                        }
            };

            var json = JsonSerializer.Serialize(newIssue);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var ytResponse = await ytClient.PostAsync($"{ytUrl}/api/issues?fields=idReadable,summary", content);

            if (ytResponse.IsSuccessStatusCode)
            {
                var doc = JsonDocument.Parse(await ytResponse.Content.ReadAsStringAsync());
                string idReadable = doc.RootElement.GetProperty("idReadable").GetString()!;
                Console.WriteLine($"Created issue {idReadable} successfully.");
            }
            else
            {
                string error = await ytResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to create issue '{newIssue.Summary}': {ytResponse.StatusCode}");
                Console.WriteLine(error);
            }
        }

        public string MapGitHubStateToYouTrack(string ghState)
        {
            return ghState.ToLower() switch
            {
                "open" => "Open",
                "closed" => "Fixed",
                _ => "Open"
            };
        }
    }
}
