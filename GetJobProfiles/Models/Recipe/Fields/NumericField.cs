namespace GetJobProfiles.Models.Recipe.Fields
{
    public class NumericField
    {
        public NumericField(decimal val) => Value = val;

        public decimal Value { get; set; }
    }
}
