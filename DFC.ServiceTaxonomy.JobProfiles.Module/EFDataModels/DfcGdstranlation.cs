﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels
{
    [Table("DFC_GDSTranlations")]
    public partial class DfcGdstranlation
    {
        [Key]
        [Column("onet_element_id")]
        [StringLength(20)]
        public string OnetElementId { get; set; }
        [Column("translation")]
        [StringLength(1500)]
        public string Translation { get; set; }
        [Column("datetimestamp", TypeName = "datetime")]
        public DateTime Datetimestamp { get; set; }
    }
}
