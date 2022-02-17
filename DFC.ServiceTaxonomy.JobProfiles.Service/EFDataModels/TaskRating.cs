﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.EFDataModels
{
    [Table("task_ratings")]
    public partial class TaskRating
    {
        [Required]
        [Column("onetsoc_code")]
        [StringLength(10)]
        public string OnetsocCode { get; set; }
        [Column("task_id", TypeName = "decimal(8, 0)")]
        public decimal TaskId { get; set; }
        [Required]
        [Column("scale_id")]
        [StringLength(3)]
        public string ScaleId { get; set; }
        [Column("category", TypeName = "decimal(3, 0)")]
        public decimal? Category { get; set; }
        [Column("data_value", TypeName = "decimal(5, 2)")]
        public decimal DataValue { get; set; }
        [Column("n", TypeName = "decimal(4, 0)")]
        public decimal? N { get; set; }
        [Column("standard_error", TypeName = "decimal(5, 2)")]
        public decimal? StandardError { get; set; }
        [Column("lower_ci_bound", TypeName = "decimal(5, 2)")]
        public decimal? LowerCiBound { get; set; }
        [Column("upper_ci_bound", TypeName = "decimal(5, 2)")]
        public decimal? UpperCiBound { get; set; }
        [Column("recommend_suppress")]
        [StringLength(1)]
        public string RecommendSuppress { get; set; }
        [Column("date_updated", TypeName = "date")]
        public DateTime DateUpdated { get; set; }
        [Required]
        [Column("domain_source")]
        [StringLength(30)]
        public string DomainSource { get; set; }

        [ForeignKey(nameof(OnetsocCode))]
        public virtual OccupationDatum OnetsocCodeNavigation { get; set; }
        [ForeignKey(nameof(ScaleId))]
        public virtual ScalesReference Scale { get; set; }
        [ForeignKey(nameof(TaskId))]
        public virtual TaskStatement Task { get; set; }
        [ForeignKey("ScaleId,Category")]
        public virtual TaskCategory TaskCategory { get; set; }
    }
}