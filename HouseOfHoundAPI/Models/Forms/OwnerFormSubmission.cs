using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Forms
{
    public class OwnerFormSubmission
    {
        public int OwnerFormSubmissionId { get; set; }
        public int OwnerId { get; set; }
        public int OwnerFormTemplateId { get; set; }

        public string JsonData { get; set; }
        public DateTime SubmittedUtc { get; set; }
    }
}