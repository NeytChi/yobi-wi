using System;
using System.Collections.Generic;

namespace YobiWi.Development.Models
{
    public partial class Files
    {
        public long fileId { get; set; }
        public string filePath { get; set; }
        public string fileName { get; set; }
        public string fileType { get; set; }
        public string fileExtension { get; set; }
    }
}
