using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using OrchardCore.Entities;

namespace GetJobProfiles
{
    public class QCFLevelBuilder
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();
        public IEnumerable<QCFLevelContentItem> QCFLevelContentItems { get; private set; }
        public Dictionary<string, string> QCFLevelDictionary { get; private set; }

        public void Build(string timestamp)
        {
            InitialiseDictionary();

            var qcfLevelList = new List<QCFLevelContentItem>();
            qcfLevelList.Add(new QCFLevelContentItem("0", timestamp, QCFLevelDictionary["0"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Entry Level") } });
            qcfLevelList.Add(new QCFLevelContentItem("1", timestamp, QCFLevelDictionary["1"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 1") } });
            qcfLevelList.Add(new QCFLevelContentItem("2", timestamp, QCFLevelDictionary["2"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 2") } });
            qcfLevelList.Add(new QCFLevelContentItem("3", timestamp, QCFLevelDictionary["3"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 3") } });
            qcfLevelList.Add(new QCFLevelContentItem("4", timestamp, QCFLevelDictionary["4"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 4") } });
            qcfLevelList.Add(new QCFLevelContentItem("5", timestamp, QCFLevelDictionary["5"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 5") } });
            qcfLevelList.Add(new QCFLevelContentItem("6", timestamp, QCFLevelDictionary["6"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 6") } });
            qcfLevelList.Add(new QCFLevelContentItem("7", timestamp, QCFLevelDictionary["7"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 7") } });
            qcfLevelList.Add(new QCFLevelContentItem("8", timestamp, QCFLevelDictionary["8"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("Qualification Level 8") } });
            qcfLevelList.Add(new QCFLevelContentItem("99", timestamp, QCFLevelDictionary["99"]) { EponymousPart = new Models.Recipe.Parts.QCFLevelPart { Description = new HtmlField("No qualifications") } });

            QCFLevelContentItems = qcfLevelList;
        }

        private void InitialiseDictionary()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("0", _generator.GenerateUniqueId());
            dictionary.Add("1", _generator.GenerateUniqueId());
            dictionary.Add("2", _generator.GenerateUniqueId());
            dictionary.Add("3", _generator.GenerateUniqueId());
            dictionary.Add("4", _generator.GenerateUniqueId());
            dictionary.Add("5", _generator.GenerateUniqueId());
            dictionary.Add("6", _generator.GenerateUniqueId());
            dictionary.Add("7", _generator.GenerateUniqueId());
            dictionary.Add("8", _generator.GenerateUniqueId());
            dictionary.Add("99", _generator.GenerateUniqueId());

            QCFLevelDictionary = dictionary;
        }
    }
}
