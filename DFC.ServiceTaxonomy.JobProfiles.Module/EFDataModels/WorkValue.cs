﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("work_values")]
    public partial class WorkValue
    {
        [Required]
        [Column("onetsoc_code")]
        [StringLength(10)]
        public string OnetsocCode { get; set; }
        [Required]
        [Column("element_id")]
        [StringLength(20)]
        public string ElementId { get; set; }
        [Required]
        [Column("scale_id")]
        [StringLength(3)]
        public string ScaleId { get; set; }
        [Column("data_value", TypeName = "decimal(5, 2)")]
        public decimal DataValue { get; set; }
        [Column("date_updated", TypeName = "date")]
        public DateTime DateUpdated { get; set; }
        [Required]
        [Column("domain_source")]
        [StringLength(30)]
        public string DomainSource { get; set; }

        [ForeignKey(nameof(ElementId))]
        public virtual ContentModelReference Element { get; set; }
        [ForeignKey(nameof(OnetsocCode))]
        public virtual OccupationDatum OnetsocCodeNavigation { get; set; }
        [ForeignKey(nameof(ScaleId))]
        public virtual ScalesReference Scale { get; set; }
    }
}
