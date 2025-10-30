using GitHubAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitHubAPI.Helpers
{
    public class UpdateYouTrackIssue
    {
        public async Task UpdateIssue(GitHubIssue gIssue, HttpClient ytClient, string ytUrl, IssueWithTypedFields typedIssue)
        {
            var issueUpdate = new
            {
                summary = gIssue.title,
                description = gIssue.body,
                customFields = new[]
                        {
                            new CustomTypedField
                            {
                                Name = "State",
                                Type = "SingleEnumIssueCustomField",
                                Value = new FieldValue
                                {
                                    Type = "StateBundleElement",
                                    Name = gIssue.state == "open" ? "Open" : "Fixed"
                                }
                            }
                        }
            };

            var json = JsonSerializer.Serialize(issueUpdate);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var ytUpdateResponse = await ytClient.PostAsync(
                $"{ytUrl}/api/issues/{typedIssue.IdReadable}?fields=idReadable,summary,description,customFields(name,value(name))",
                content
            );

            if (ytUpdateResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Updated YouTrack issue {typedIssue.IdReadable} successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to update issue {typedIssue.IdReadable}: {ytUpdateResponse.StatusCode}");
                Console.WriteLine(await ytUpdateResponse.Content.ReadAsStringAsync());
            }
        }
    }
}
