﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("occupation_level_metadata")]
    public partial class OccupationLevelMetadatum
    {
        [Required]
        [Column("onetsoc_code")]
        [StringLength(10)]
        public string OnetsocCode { get; set; }
        [Required]
        [Column("item")]
        [StringLength(150)]
        public string Item { get; set; }
        [Column("response")]
        [StringLength(75)]
        public string Response { get; set; }
        [Column("n", TypeName = "decimal(4, 0)")]
        public decimal? N { get; set; }
        [Column("percent", TypeName = "decimal(4, 1)")]
        public decimal? Percent { get; set; }
        [Column("date_updated", TypeName = "date")]
        public DateTime DateUpdated { get; set; }

        [ForeignKey(nameof(OnetsocCode))]
        public virtual OccupationDatum OnetsocCodeNavigation { get; set; }
    }
}
