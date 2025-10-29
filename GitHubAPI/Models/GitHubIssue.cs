using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubAPI.Models
{
    public class GitHubIssue
    {
        public long id { get; set; }
        public int number { get; set; }
        public string? title { get; set; }
        public string? state { get; set; }
        public string? body { get; set; }
    }
}
