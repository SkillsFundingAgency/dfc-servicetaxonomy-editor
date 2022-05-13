﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("occupation_data")]
    public partial class OccupationDatum
    {
        public OccupationDatum()
        {
            TaskStatements = new HashSet<TaskStatement>();
        }

        [Key]
        [Column("onetsoc_code")]
        [StringLength(10)]
        public string OnetsocCode { get; set; }
        [Required]
        [Column("title")]
        [StringLength(150)]
        public string Title { get; set; }
        [Required]
        [Column("description")]
        [StringLength(1000)]
        public string Description { get; set; }

        [InverseProperty(nameof(TaskStatement.OnetsocCodeNavigation))]
        public virtual ICollection<TaskStatement> TaskStatements { get; set; }
    }
}
