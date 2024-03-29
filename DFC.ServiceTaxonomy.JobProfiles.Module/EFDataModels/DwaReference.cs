﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("dwa_reference")]
    public partial class DwaReference
    {
        [Required]
        [Column("element_id")]
        [StringLength(20)]
        public string ElementId { get; set; }
        [Required]
        [Column("iwa_id")]
        [StringLength(20)]
        public string IwaId { get; set; }
        [Key]
        [Column("dwa_id")]
        [StringLength(20)]
        public string DwaId { get; set; }
        [Required]
        [Column("dwa_title")]
        [StringLength(150)]
        public string DwaTitle { get; set; }

        [ForeignKey(nameof(ElementId))]
        [InverseProperty(nameof(ContentModelReference.DwaReferences))]
        public virtual ContentModelReference Element { get; set; }
        [ForeignKey(nameof(IwaId))]
        [InverseProperty(nameof(IwaReference.DwaReferences))]
        public virtual IwaReference Iwa { get; set; }
    }
}
