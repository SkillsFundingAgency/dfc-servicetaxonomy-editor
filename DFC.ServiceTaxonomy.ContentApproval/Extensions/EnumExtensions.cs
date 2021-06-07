using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DFC.ServiceTaxonomy.ContentApproval.Extensions
{
    public static class EnumExtensions
    {
        //todo: move out of this class?
        public static IEnumerable<SelectListItem> GetSelectList(Type enumType)
        {
            FieldInfo[] enumFields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            return enumFields.Select(fi => (fi, da:fi.GetCustomAttribute<DisplayAttribute>()))
                .OrderBy(v => v.da?.Order ?? -1)
                .Select(v => new SelectListItem(v.da?.Name ?? v.fi.Name, v.fi.Name));
        }

        public static Dictionary<string, string> GetEnumNameAndDisplayNameDictionary(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException($"{enumType.Name} is not an enum.");
            }

            FieldInfo[] enumFields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return enumFields.ToDictionary(k => k.Name, v => v.GetCustomAttribute<DisplayAttribute>()?.Name ?? v.Name);
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            MemberInfo? enumValueMemberInfo = enumValue.GetType().GetMember(enumValue.ToString()).First();
            return enumValueMemberInfo.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? enumValueMemberInfo.Name;
        }
    }
}
