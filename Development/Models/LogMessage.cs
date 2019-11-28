namespace YobiWi.Development.Models
{
    public enum LogLevel 
    { 
        DEBUG, 
        INFO, 
        WARN, 
        ERROR, 
        FATAL,
        OFF,
        TRACE
    }

    public partial class LogMessage
    {
        public long logId { get; set; }
        public string message { get; set; }
        public string userComputer { get; set; }
        public System.DateTime createdAt { get; set; }
        public string level { get; set; }
        public long userId { get; set; }
        public long threadId { get; set; }
        public string userIP { get; set; }
    }
}
