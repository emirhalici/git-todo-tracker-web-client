namespace git_todo_tracker_web_client.Models
{
    public class RepositoryModel
    {
        public required string Id { get; set; }
        public string BaseUrl { get; set; } = "https://github.com";
        public required string UserName { get; set; }
        public required string ProjectName { get; set; }

        public string FullGitLink
        {
            get => $"{BaseUrl}/{UserName}/{ProjectName}";
        }
    }
}