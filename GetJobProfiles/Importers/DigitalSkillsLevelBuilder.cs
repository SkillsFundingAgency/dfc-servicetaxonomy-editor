using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using OrchardCore.Entities;


namespace GetJobProfiles.Importers
{
    public class DigitalSkillsLevelBuilder
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();

        public IEnumerable<DigitalSkillsLevelContentItem> DigitalSkillsLevelContentItems { get; private set; }

        public Dictionary<string, string> DigitalSkillLevelDictionary { get; private set; }

        private string[] digitalSkillLevels =
        {
            "to have a thorough understanding of computer systems and applications",
            "to be able to use a computer and the main software packages confidently",
            "to be able to use a computer and the main software packages competently",
            "to be able to carry out basic tasks on a computer or hand-held device"
        };

        public DigitalSkillsLevelBuilder Build(string timestamp)
        {

            InitialiseDictionary();

            var digitalSkillLevelList = new List<DigitalSkillsLevelContentItem>();

            digitalSkillLevelList.Add(new DigitalSkillsLevelContentItem(digitalSkillLevels[0], timestamp, DigitalSkillLevelDictionary[digitalSkillLevels[0]])
            {
                EponymousPart = new Models.Recipe.Parts.DigitalSkillsLevelPart
                {
                    Description = new TextField(digitalSkillLevels[0])
                }
            });

            digitalSkillLevelList.Add(new DigitalSkillsLevelContentItem(digitalSkillLevels[1], timestamp, DigitalSkillLevelDictionary[digitalSkillLevels[1]])
            {
                EponymousPart = new Models.Recipe.Parts.DigitalSkillsLevelPart
                {
                    Description = new TextField(digitalSkillLevels[1])
                }
            });

            digitalSkillLevelList.Add(new DigitalSkillsLevelContentItem(digitalSkillLevels[2], timestamp, DigitalSkillLevelDictionary[digitalSkillLevels[2]])
            {
                EponymousPart = new Models.Recipe.Parts.DigitalSkillsLevelPart
                {
                    Description = new TextField(digitalSkillLevels[2])
                }
            });

            digitalSkillLevelList.Add(new DigitalSkillsLevelContentItem(digitalSkillLevels[3], timestamp, DigitalSkillLevelDictionary[digitalSkillLevels[3]])
            {
                EponymousPart = new Models.Recipe.Parts.DigitalSkillsLevelPart
                {
                    Description = new TextField(digitalSkillLevels[3])
                }
            });

            DigitalSkillsLevelContentItems = digitalSkillLevelList;

            return this;
        }

        private void InitialiseDictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                {digitalSkillLevels[0], _generator.GenerateUniqueId()},
                {digitalSkillLevels[1], _generator.GenerateUniqueId()},
                {digitalSkillLevels[2], _generator.GenerateUniqueId()},
                {digitalSkillLevels[3], _generator.GenerateUniqueId()},
            };

            DigitalSkillLevelDictionary = dictionary;
        }
    }
}
