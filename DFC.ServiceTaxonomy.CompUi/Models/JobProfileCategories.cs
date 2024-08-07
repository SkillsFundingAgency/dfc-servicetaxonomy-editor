﻿using System.Text.Json;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class JobProfileCategoriesContent
    {
        [JsonProperty("TitlePart")]
        public TitleDesc? TitlePart { get; set; }

        [JsonProperty("JobProfile")]
        public JobProfile? JobProfile { get; set; }
    }

    public class TitleDesc
    {
        [JsonProperty("Title")]
        public string? Title { get; set; }
    }

    public class JobProfile
    {
        [JsonProperty("JobProfileCategory")]
        public ContentItemIds? JobProfileCategory { get; set; }
    }
}
