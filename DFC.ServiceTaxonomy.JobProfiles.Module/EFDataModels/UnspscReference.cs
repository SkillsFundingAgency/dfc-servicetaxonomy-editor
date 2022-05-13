﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("unspsc_reference")]
    public partial class UnspscReference
    {
        [Key]
        [Column("commodity_code", TypeName = "decimal(8, 0)")]
        public decimal CommodityCode { get; set; }
        [Required]
        [Column("commodity_title")]
        [StringLength(150)]
        public string CommodityTitle { get; set; }
        [Column("class_code", TypeName = "decimal(8, 0)")]
        public decimal ClassCode { get; set; }
        [Required]
        [Column("class_title")]
        [StringLength(150)]
        public string ClassTitle { get; set; }
        [Column("family_code", TypeName = "decimal(8, 0)")]
        public decimal FamilyCode { get; set; }
        [Required]
        [Column("family_title")]
        [StringLength(150)]
        public string FamilyTitle { get; set; }
        [Column("segment_code", TypeName = "decimal(8, 0)")]
        public decimal SegmentCode { get; set; }
        [Required]
        [Column("segment_title")]
        [StringLength(150)]
        public string SegmentTitle { get; set; }
    }
}