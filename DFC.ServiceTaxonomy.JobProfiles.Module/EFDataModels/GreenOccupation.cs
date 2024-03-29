﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("green_occupations")]
    public partial class GreenOccupation
    {
        [Required]
        [Column("onetsoc_code")]
        [StringLength(10)]
        public string OnetsocCode { get; set; }
        [Required]
        [Column("green_occupational_category")]
        [StringLength(40)]
        public string GreenOccupationalCategory { get; set; }

        [ForeignKey(nameof(OnetsocCode))]
        public virtual OccupationDatum OnetsocCodeNavigation { get; set; }
    }
}
