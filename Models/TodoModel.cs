namespace git_todo_tracker_web_client.Models
{
    public class TodoModel
    {
        public required string Message { get; set; }
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
    }
}