﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("content_model_reference")]
    public partial class ContentModelReference
    {
        public ContentModelReference()
        {
            DwaReferences = new HashSet<DwaReference>();
            EteCategories = new HashSet<EteCategory>();
            GreenDwaReferences = new HashSet<GreenDwaReference>();
            IwaReferences = new HashSet<IwaReference>();
            WorkContextCategories = new HashSet<WorkContextCategory>();
        }

        [Key]
        [Column("element_id")]
        [StringLength(20)]
        public string ElementId { get; set; }
        [Required]
        [Column("element_name")]
        [StringLength(150)]
        public string ElementName { get; set; }
        [Required]
        [Column("description")]
        [StringLength(1500)]
        public string Description { get; set; }

        [InverseProperty(nameof(DwaReference.Element))]
        public virtual ICollection<DwaReference> DwaReferences { get; set; }
        [InverseProperty(nameof(EteCategory.Element))]
        public virtual ICollection<EteCategory> EteCategories { get; set; }
        [InverseProperty(nameof(GreenDwaReference.Element))]
        public virtual ICollection<GreenDwaReference> GreenDwaReferences { get; set; }
        [InverseProperty(nameof(IwaReference.Element))]
        public virtual ICollection<IwaReference> IwaReferences { get; set; }
        [InverseProperty(nameof(WorkContextCategory.Element))]
        public virtual ICollection<WorkContextCategory> WorkContextCategories { get; set; }
    }
}
