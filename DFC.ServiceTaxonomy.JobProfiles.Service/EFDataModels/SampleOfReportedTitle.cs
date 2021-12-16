﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.EFDataModels
{
    [Table("sample_of_reported_titles")]
    public partial class SampleOfReportedTitle
    {
        [Required]
        [Column("onetsoc_code")]
        [StringLength(10)]
        public string OnetsocCode { get; set; }
        [Required]
        [Column("reported_job_title")]
        [StringLength(150)]
        public string ReportedJobTitle { get; set; }
        [Required]
        [Column("shown_in_my_next_move")]
        [StringLength(1)]
        public string ShownInMyNextMove { get; set; }

        [ForeignKey(nameof(OnetsocCode))]
        public virtual OccupationDatum OnetsocCodeNavigation { get; set; }
    }
}