using System;
using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus
{
    internal class JobProfileMessage
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public Guid JobProfileId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the HtBTitlePrefix.
        /// </summary>
        /// <value>
        /// HtBTitlePrefix.
        /// </value>
        public string? DynamicTitlePrefix { get; set; }

        /// <summary>
        /// Gets or sets the alternative title.
        /// </summary>
        /// <value>
        /// The alternative title.
        /// </value>
        public string? AlternativeTitle { get; set; }

        /// <summary>
        /// Gets or sets the overview.
        /// </summary>
        /// <value>
        /// The overview.
        /// </value>
        public string? Overview { get; set; }

        /// <summary>
        /// Gets or sets the soc code.
        /// </summary>
        /// <value>
        /// The soc code.
        /// </value>
        public string? SocLevelTwo { get; set; }

        /// <summary>
        /// Gets or sets the name of the URL.
        /// </summary>
        /// <value>
        /// The name of the URL.
        /// </value>
        public string? UrlName { get; set; }

        public string? DigitalSkillsLevel { get; set; }

        public IEnumerable<RestrictionItem>? Restrictions { get; set; }

        public string? OtherRequirements { get; set; }

        /// <summary>
        /// Gets or sets the career path and progression.
        /// </summary>
        /// <value>
        /// The career path and progression.
        /// </value>
        public string? CareerPathAndProgression { get; set; }

        /// <summary>
        /// Gets or sets the course keywords.
        /// </summary>
        /// <value>
        /// The course keywords.
        /// </value>
        public string? CourseKeywords { get; set; }

        public decimal? MinimumHours { get; set; }

        public decimal? MaximumHours { get; set; }

        /// <summary>
        /// Gets or sets the SalaryStarter.
        /// </summary>
        /// <value>
        /// The SalaryStarter.
        /// </value>
        public decimal? SalaryStarter { get; set; }

        /// <summary>
        /// Gets or sets the SalaryExperienced.
        /// </summary>
        /// <value>
        /// The SalaryExperienced.
        /// </value>
        public decimal? SalaryExperienced { get; set; }

        public IEnumerable<Classification>? WorkingPattern { get; set; }

        public IEnumerable<Classification>? WorkingPatternDetails { get; set; }

        public IEnumerable<Classification>? WorkingHoursDetails { get; set; }

        public IEnumerable<Classification>? HiddenAlternativeTitle { get; set; }

        public IEnumerable<Classification>? JobProfileSpecialism { get; set; }

        public bool? IsImported { get; set; }

        public HowToBecomeData? HowToBecomeData { get; set; }

        public WhatYouWillDoData? WhatYouWillDoData { get; set; }

        public SocCodeItem? SocCodeData { get; set; }

        public IEnumerable<JobProfileRelatedCareerItem>? RelatedCareersData { get; set; }

        public IEnumerable<SocSkillMatrixItem>? SocSkillsMatrixData { get; set; }

        public IEnumerable<JobProfileCategoryItem>? JobProfileCategories { get; set; }

        public DateTime LastModified { get; set; }

        public string? CanonicalName { get; set; }

        public string? WidgetContentTitle { get; set; }

        public bool IncludeInSitemap { get; set; }
    }
}
