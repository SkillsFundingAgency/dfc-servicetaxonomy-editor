namespace DFC.ServiceTaxonomy.CompUi.Services
{
    public static class ItemInList
    {
        public static bool In<T>(this T val, params T[] values) where T : struct
        {
            return values.Contains(val);
        }
    }
}
