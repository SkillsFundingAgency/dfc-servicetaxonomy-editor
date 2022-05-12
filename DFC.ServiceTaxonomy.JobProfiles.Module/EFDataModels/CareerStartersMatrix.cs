﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("career_starters_matrix")]
    public partial class CareerStartersMatrix
    {
        [Required]
        [Column("onetsoc_code")]
        [StringLength(10)]
        public string OnetsocCode { get; set; }
        [Required]
        [Column("related_onetsoc_code")]
        [StringLength(10)]
        public string RelatedOnetsocCode { get; set; }
        [Column("related_index", TypeName = "decimal(3, 0)")]
        public decimal RelatedIndex { get; set; }

        [ForeignKey(nameof(OnetsocCode))]
        public virtual OccupationDatum OnetsocCodeNavigation { get; set; }
        [ForeignKey(nameof(RelatedOnetsocCode))]
        public virtual OccupationDatum RelatedOnetsocCodeNavigation { get; set; }
    }
}
