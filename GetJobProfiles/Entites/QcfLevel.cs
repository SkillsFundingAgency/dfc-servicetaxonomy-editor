using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using OrchardCore.Entities;

namespace GetJobProfiles.Entities
{
    public class QcfLevel
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();

        public IEnumerable<QcfLevelContentItem> QcfLevelContentItems { get; private set; }

        public Dictionary<string, string> QcfLevelDictionary { get; private set; }

        public QcfLevel Build(string timestamp)
        {
            InitialiseDictionary();

            var qcfLevelList = new List<QcfLevelContentItem>();
            qcfLevelList.Add(new QcfLevelContentItem("0", timestamp, QcfLevelDictionary["0"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Entry Level") } });
            qcfLevelList.Add(new QcfLevelContentItem("1", timestamp, QcfLevelDictionary["1"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 1") } });
            qcfLevelList.Add(new QcfLevelContentItem("2", timestamp, QcfLevelDictionary["2"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 2") } });
            qcfLevelList.Add(new QcfLevelContentItem("3", timestamp, QcfLevelDictionary["3"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 3") } });
            qcfLevelList.Add(new QcfLevelContentItem("4", timestamp, QcfLevelDictionary["4"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 4") } });
            qcfLevelList.Add(new QcfLevelContentItem("5", timestamp, QcfLevelDictionary["5"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 5") } });
            qcfLevelList.Add(new QcfLevelContentItem("6", timestamp, QcfLevelDictionary["6"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 6") } });
            qcfLevelList.Add(new QcfLevelContentItem("7", timestamp, QcfLevelDictionary["7"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 7") } });
            qcfLevelList.Add(new QcfLevelContentItem("8", timestamp, QcfLevelDictionary["8"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 8") } });
            qcfLevelList.Add(new QcfLevelContentItem("99", timestamp, QcfLevelDictionary["99"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("No qualifications") } });

            QcfLevelContentItems = qcfLevelList;

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

            QcfLevelDictionary = dictionary;
        }
    }
}
