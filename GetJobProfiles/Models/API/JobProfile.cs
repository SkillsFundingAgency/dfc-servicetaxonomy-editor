using System.Linq;

namespace GetJobProfiles.Models.API
{
    public class JobProfile
    {
        public string Title;
        public string LastUpdatedDate;
        public string Url;
        public string Soc;
        public string ONetOccupationalCode;
        public string AlternativeTitle; // csv. we'll probably ignore these, and serve up esco's alt labels instead
        public string Overview;
        public string SalaryStarter;
        public string SalaryExperienced;
        public decimal MinimumHours; // are there any 0.5?s if not, can just be int
        public decimal MaximumHours;
        public string WorkingHoursDetails;
        public string WorkingPattern;
        public string WorkingPatternDetails;
        public HowToBecome HowToBecome;
        public WhatItTakes WhatItTakes;
        public WhatYouWillDo WhatYouWillDo;
        public CareerPathAndProgressionType CareerPathAndProgression;
        public RelatedCareer[] RelatedCareers;
    }

    public class HowToBecome
    {
        // don't need this (why array?)
        //public string[] EntryRouteSummary;

        // object, not array
        public EntryRoutes EntryRoutes;
        public MoreInformation MoreInformation;
    }

    public class EntryRoutes
    {
        public AcademicEntryRoute University;
        public AcademicEntryRoute College;
        public AcademicEntryRoute Apprenticeship;
        public string[] Work;
        public string[] Volunteering;
        public string[] DirectApplication;
        public string[] OtherRoutes;
    }

    public class AcademicEntryRoute
    {
        public string[] RelevantSubjects;
        public string[] FurtherInformation;
        public string EntryRequirementPreface;
        public string[] EntryRequirements;
        public string[] AdditionalInformation;

        public bool IsEmpty()
        {
            return !RelevantSubjects.Any()
                   && !FurtherInformation.Any()
                   && EntryRequirementPreface == null
                   && !EntryRequirements.Any()
                   && !AdditionalInformation.Any();
        }
    }

    public class MoreInformation
    {
        public string[] Registrations;                    // ?
        public string[] CareerTips;                       // real array, each string "[anchor test : url]"
        public string[] ProfessionalAndIndustryBodies;    // ?
        public string[] FurtherInformation;               // real array, each string "[anchor test : url]"
    }

    public class WhatItTakes
    {
        public string DigitalSkillsLevel;
        public Skill[] Skills;
        public RestrictionsAndRequirements RestrictionsAndRequirements;
    }

    public class Skill
    {
        public string Description;
        public string ONetAttributeType;
        public string ONetRank;                // really a decimal??
        public string ONetElementId;
    }

    public class RestrictionsAndRequirements
    {
        public string[] RelatedRestrictions;        // todo: array of what?
        public string[] OtherRequirements;        // todo: array of what?
    }

    public class WhatYouWillDo
    {
        public string[] WYDDayToDayTasks;        // just a string?
        public WorkingEnvironment workingEnvironment;
    }

    public class WorkingEnvironment
    {
        public string Location;
        public string Environment;
        public string Uniform;
    }

    public class CareerPathAndProgressionType
    {
        public string[] CareerPathAndProgression;    // real array
    }

    public class RelatedCareer
    {
        public string Title;
        public string Url;
    }
}
