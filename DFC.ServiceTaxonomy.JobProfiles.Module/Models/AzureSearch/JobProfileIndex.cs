using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling.Attributes;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.AzureSearch
{
    public class JobProfileIndex
    {
        [Key]
        [SimpleField(IsKey = true, IsHidden = true)]
        public string IdentityField { get; set; } = string.Empty;

        [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string SocCode { get; set; } = string.Empty;

        [IsSuggestible]
        [SearchableField(IsFilterable = true, IsSortable = true, AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        [AddWeighting(7)]
        public string Title { get; set; } = string.Empty;

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        [AddWeighting(100)]
        public string TitleAsKeyword => Title.ToLower();

        [IsSuggestible]
        [SearchableField(IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        [AddWeighting(6)]
        public IEnumerable<string> AlternativeTitle { get; set; } = Enumerable.Empty<string>();

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        [AddWeighting(100)]
        public IEnumerable<string> AltTitleAsKeywords => AlternativeTitle.Select(a => a.ToLower());

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        [AddWeighting(100)]
        public string Overview { get; set; } = string.Empty;

        [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public double SalaryStarter { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public double SalaryExperienced { get; set; }

        [SimpleField(IsFilterable = true)]
        public string UrlName { get; set; } = string.Empty;

        [SearchableField(IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        [AddWeighting(4)]
        public IEnumerable<string> JobProfileCategories { get; set; } = Enumerable.Empty<string>();

        [SearchableField(IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        [AddWeighting(3)]
        public IEnumerable<string> JobProfileSpecialism { get; set; } = Enumerable.Empty<string>();

        [SearchableField(IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        [AddWeighting(3)]
        public IEnumerable<string> HiddenAlternativeTitle { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<string> JobProfileCategoriesWithUrl { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> JobProfileCategoryUrls { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> Interests { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> Enablers { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> EntryQualifications { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> TrainingRoutes { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> PreferredTaskTypes { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> JobAreas { get; set; } = Enumerable.Empty<string>();

        [SearchableField(IsFilterable = true)]
        public IEnumerable<string> Skills { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true, IsSortable = true)]
        public double EntryQualificationLowestLevel { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        public string CollegeRelevantSubjects { get; set; } = string.Empty;

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        public string UniversityRelevantSubjects { get; set; } = string.Empty;

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        public string ApprenticeshipRelevantSubjects { get; set; } = string.Empty;

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        public string WYDDayToDayTasks { get; set; } = string.Empty;

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        public string CareerPathAndProgression { get; set; } = string.Empty;

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> WorkingPattern { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> WorkingPatternDetails { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true)]
        public IEnumerable<string> WorkingHoursDetails { get; set; } = Enumerable.Empty<string>();

        [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public double MinimumHours { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public double MaximumHours { get; set; }
    }

}
