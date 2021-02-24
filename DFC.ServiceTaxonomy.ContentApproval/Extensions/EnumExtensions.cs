using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DFC.ServiceTaxonomy.ContentApproval.Extensions
{
    public static class EnumExtensions
    {
        public static string[] GetDisplayNames(Type enumType)
        {
            // we need to match Enum.GetNames (we combine the two results with zip)
            FieldInfo[] enumFields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            return enumFields.Select(fi =>
                {
                    DisplayAttribute? displayAttribute = fi.GetCustomAttribute<DisplayAttribute>();
                    return displayAttribute?.Name ?? fi.Name;
                })
                .ToArray();
        }
    }
}
