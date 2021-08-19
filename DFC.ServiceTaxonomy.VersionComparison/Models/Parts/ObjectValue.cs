using System;

namespace DFC.ServiceTaxonomy.VersionComparison.Models.Parts
{
    public class ObjectValue
    {
        public string? Html { get; set; }
        public string? Text { get; set; }
        public string? Value { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Html))
            {
                return Html;
            }
            if (!string.IsNullOrWhiteSpace(Text))
            {
                if (Text.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                    Text.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Text.Equals("true", StringComparison.InvariantCultureIgnoreCase) ? "Yes" : "No";
                }
                return Text;
            }
            if (!string.IsNullOrWhiteSpace(Value))
            {
                if (Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                    Value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ? "Yes" : "No";
                }
                return Value;
            }

            return string.Empty;
        }
    }
}
