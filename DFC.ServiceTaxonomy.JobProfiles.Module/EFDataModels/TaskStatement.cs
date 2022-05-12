﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("task_statements")]
    public partial class TaskStatement
    {
        [Required]
        [Column("onetsoc_code")]
        [StringLength(10)]
        public string OnetsocCode { get; set; }
        [Key]
        [Column("task_id", TypeName = "decimal(8, 0)")]
        public decimal TaskId { get; set; }
        [Required]
        [Column("task")]
        [StringLength(1000)]
        public string Task { get; set; }
        [Column("task_type")]
        [StringLength(12)]
        public string TaskType { get; set; }
        [Column("incumbents_responding", TypeName = "decimal(4, 0)")]
        public decimal? IncumbentsResponding { get; set; }
        [Column("date_updated", TypeName = "date")]
        public DateTime DateUpdated { get; set; }
        [Required]
        [Column("domain_source")]
        [StringLength(30)]
        public string DomainSource { get; set; }

        [ForeignKey(nameof(OnetsocCode))]
        [InverseProperty(nameof(OccupationDatum.TaskStatements))]
        public virtual OccupationDatum OnetsocCodeNavigation { get; set; }
    }
}
