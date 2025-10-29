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



            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);

            var response = await client.GetAsync($"https://api.github.com/repos/{repo}/issues");
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var issues = JsonSerializer.Deserialize<List<GitHubIssue>>(json);

            List<YouTrackTask> tasks = issues.Select(i => new YouTrackTask
            {
               Id = i.number.ToString(),
               Title = i.title,
               Status = i.state,
               Description = i.body
            }).ToList();

            using HttpClient ytClient = new HttpClient();
            ytClient.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");
            ytClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ytToken);

            foreach (var task in tasks)
            {
                var issueData = new Issue
                {
                    Project = new ProjectRef { Id = ytProject },
                    Summary = task.Title,
                    Description = task.Description,
                    CustomFields = new[]
                    {
                        new CustomField
                        {
                            Name = "State",
                            Value = new FieldValue { Name = task.Status }
                        }
                    }
                };

                json = JsonSerializer.Serialize(issueData);
                Console.WriteLine(json);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var ytResponse = await ytClient.PostAsync($"{ytUrl}/api/issues?fields=idReadable,summary", content);

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
        }
    }
}
