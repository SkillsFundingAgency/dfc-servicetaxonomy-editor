using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using OrchardCore.Entities;

namespace GetJobProfiles.Entities
{
    public class DigitalSkillsLevelContentItemBuilder
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();
        public IEnumerable<QcfLevelContentItem> QCFLevelContentItems { get; private set; }
        public Dictionary<string, string> QCFLevelDictionary { get; private set; }

        //public IEnumerable<QCFLevelContentItem> Build(string timestamp)
        public DigitalSkillsLevelContentItemBuilder Build(string timestamp)
        {
            InitialiseDictionary();

            //var digitalSkillsLevelContentItemList = new List<DigitalSkillsLevelContentItem>();
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("0", timestamp, QCFLevelDictionary["0"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Entry Level") } });
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("1", timestamp, QCFLevelDictionary["1"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 1") } });
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("2", timestamp, QCFLevelDictionary["2"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 2") } });
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("3", timestamp, QCFLevelDictionary["3"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 3") } });
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("4", timestamp, QCFLevelDictionary["4"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 4") } });
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("5", timestamp, QCFLevelDictionary["5"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 5") } });
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("6", timestamp, QCFLevelDictionary["6"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 6") } });
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("7", timestamp, QCFLevelDictionary["7"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 7") } });
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("8", timestamp, QCFLevelDictionary["8"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 8") } });
            //digitalSkillsLevelContentItemList.Add(new DigitalSkillsLevelContentItem("99", timestamp, QCFLevelDictionary["99"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("No qualifications") } });

            //QCFLevelContentItems = qcfLevelList;

            return this;
        }

        private void InitialiseDictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"0", _generator.GenerateUniqueId()},
                {"1", _generator.GenerateUniqueId()},
                {"2", _generator.GenerateUniqueId()},
                {"3", _generator.GenerateUniqueId()},
                {"4", _generator.GenerateUniqueId()},
                {"5", _generator.GenerateUniqueId()},
                {"6", _generator.GenerateUniqueId()},
                {"7", _generator.GenerateUniqueId()},
                {"8", _generator.GenerateUniqueId()},
                {"99", _generator.GenerateUniqueId()}
            };

            QCFLevelDictionary = dictionary;
        }
    }
}
