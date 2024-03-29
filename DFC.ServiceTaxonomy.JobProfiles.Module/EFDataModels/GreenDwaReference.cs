﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("green_dwa_reference")]
    public partial class GreenDwaReference
    {
        [Required]
        [Column("element_id")]
        [StringLength(20)]
        public string ElementId { get; set; }
        [Key]
        [Column("green_dwa_id")]
        [StringLength(20)]
        public string GreenDwaId { get; set; }
        [Required]
        [Column("green_dwa_title")]
        [StringLength(150)]
        public string GreenDwaTitle { get; set; }

        [ForeignKey(nameof(ElementId))]
        [InverseProperty(nameof(ContentModelReference.GreenDwaReferences))]
        public virtual ContentModelReference Element { get; set; }
    }
}
