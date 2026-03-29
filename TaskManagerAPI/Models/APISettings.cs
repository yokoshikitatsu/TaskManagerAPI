namespace TaskManagerAPI.Models
{
    public class APISettings
    {
        public int DefaultPageSize { get; set; } = 10;
        public int MaxPageSize { get; set; } = 50;
        public string ApiVersion { get; set; } = "v1";
    }
}
