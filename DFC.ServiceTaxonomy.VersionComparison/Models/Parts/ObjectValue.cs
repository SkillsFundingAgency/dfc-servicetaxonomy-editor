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
                if (bool.TryParse(Text, out bool result))
                {
                    return result ? "Yes" : "No";
                }
                return Text;
            }
            if (!string.IsNullOrWhiteSpace(Value))
            {
                if (bool.TryParse(Value, out bool result))
                {
                    return result ? "Yes" : "No";
                }
                return Value;
            }

            return string.Empty;
        }
    }
}
