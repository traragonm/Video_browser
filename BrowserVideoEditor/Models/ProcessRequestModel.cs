namespace BrowserVideoEditor.Models
{
    public class ProcessRequestModel
    {
        public string urls { get; set; }
        public string saveFolderPath { get; set; }
        public int splitDuration { get; set; }
        public int splitSpace { get; set; }
    }
}
