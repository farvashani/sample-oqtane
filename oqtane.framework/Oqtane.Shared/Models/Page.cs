﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Page : IAuditable, IDeletable
    {
        public int PageId { get; set; }
        public int SiteId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public int Order { get; set; }
        public string Url { get; set; }
        public string ThemeType { get; set; }
        public string LayoutType { get; set; }
        public string DefaultContainerType { get; set; }
        public string Icon { get; set; }
        public bool IsNavigation { get; set; }
        public int? UserId { get; set; }
        public bool IsPersonalizable { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }

        [NotMapped]
        public List<string> Panes { get; set; }
        [NotMapped]
        public List<Resource> Resources { get; set; }
        [NotMapped]
        public string Permissions { get; set; }
        [NotMapped]
        public int Level { get; set; }
        [NotMapped]
        public bool HasChildren { get; set; }

        [Obsolete("This property is obsolete", false)]
        [NotMapped]
        public bool EditMode { get; set; }
    }
}
