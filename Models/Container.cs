namespace ContainerInspectionApp.Models
{
    public class Container
    {
        public int Id { get; set; }
        public string ContainerId { get; set; } = string.Empty;
        public string ContainerType { get; set; } = string.Empty;
        public string Contents { get; set; } = string.Empty;
        public DateTime? DateAdded { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}