using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class PersonVM
    {
        public long Id { get; set; }
        public long CareerId { get; set; }
        public string Enrollment { get; set; }
        public string Name { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Nullable<long> Phone { get; set; }
        public string Photo { get; set; }
        public System.DateTime LastLogin { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        // CUSTOM ATTRIBUTES
        public long PermissionID { get; set; }
        public List<SubjectVM> Subjects { get; set; }
        public List<ProjectVM> Projects { get; set; }
        public string UserFullName { get; set; }
        public bool ProjectOwner { get; set; }
        public string Career { get; set; }
        public string CareerName { get; set; }
        public string PermissionName { get; set; }
        public HttpPostedFileBase File { get; set; }
        public string LastLoginFormatted { get; set; }
        public string TimeCreatedFormatted { get; set; }
    }
}