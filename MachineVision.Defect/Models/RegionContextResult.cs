namespace MachineVision.Defect.Models
{
    public class RegionContextResult
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public string Name { get; set; }

        public RectangleLocation Location { get; set; }

        public LightAndDarkRegion Render { get; set; }
    }
}
