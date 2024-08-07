namespace ContainerInspectionApp.Models
{
    public class Container
    {
        public string ContainerId { get; set; } = string.Empty;
        public string ContainerType { get; set; } = string.Empty;
        public string Contents { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
    }
}