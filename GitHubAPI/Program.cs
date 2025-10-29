using System.Net.Http.Headers;
using System.Text.Json;
using DotNetEnv;

namespace GitHubAPI
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            string repo = "JugJugJaviSe/GitHubToYouTrack";
            string envPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\.env");
            Env.Load(envPath);

            string? token = Environment.GetEnvironmentVariable("GITHUB_PAT");
            Console.WriteLine(token ?? "Token not found");

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);

            var response = await client.GetAsync($"https://api.github.com/repos/{repo}/issues");
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();




        }

        public class GitHubIssue
        {
            public int id { get; set; }
            public int number { get; set; }
            public string? title { get; set; }
            public string? state { get; set; }
            public string? body { get; set; }
        }

        public class YouTrackTask
        {
            public string? Id { get; set; }
            public string? Title { get; set; }
            public string? Status { get; set; }
            public string? Description { get; set; }
        }
    }
}
