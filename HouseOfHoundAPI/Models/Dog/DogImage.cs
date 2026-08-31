using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Dog
{
    public class DogImage
    {
        public int Id { get; set; }
        public int DogId { get; set; }

        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }

        public long? FileSizeBytes { get; set; }

        public string Note { get; set; }

        public DateTime UploadedDateUTC { get; set; }

        public bool IsActive { get; set; }
    }
}