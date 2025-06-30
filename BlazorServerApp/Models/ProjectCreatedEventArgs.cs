namespace BlazorServerApp.Models
{
    public class ProjectCreatedEventArgs
    {
        public string FileName { get; set; }
        public string Language { get; set; }
        public string Extension { get; set; }
    }
}
