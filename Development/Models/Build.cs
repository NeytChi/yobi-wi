namespace YobiWi.Development.Models
{
    public partial class Build
    {
        public int buildId { get; set; }
        public int userId { get; set; }
        public string buildName { get; set; }
        public string archiveName { get; set; }
        public string buildHash { get; set; }
        public string urlArchive { get; set; }
        public string urlInstall { get; set; }
        public string urlIcon { get; set; }
        public string version { get; set; }
        public string numberBuild { get; set; }
        public string bundleIdentifier { get; set; }
        public long createdAt { get; set; }
        public bool buildDeleted { get; set; }
    }
}
