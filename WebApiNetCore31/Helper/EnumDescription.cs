using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Helper
{
    public class EnumDescription
    {
        public static (string, string) GetDescription<T>(T @enum)
            where T : Enum
        {
            var attributes = GetDescriptionAttributes<T>(m => m.Name.ToLowerInvariant() == @enum.ToString()?.ToLowerInvariant());
            return (attributes[0].Item1.ToString(), attributes[0].Item2.Description);
        }

        public static IList<(string, string)> GetDescriptions<T>()
           where T : Enum
        {
            var attributes = GetDescriptionAttributes<T>();
            return attributes.Select(p => (p.Item1.ToString(), p.Item2.Description)).ToList();
        }

        private static List<(T, DescriptionAttribute)> GetDescriptionAttributes<T>(Func<MemberInfo, bool> predicate = null)
                where T : Enum
        {
            var enumType = typeof(T);
            var memberInfos = enumType.GetMembers().Where(m => m.DeclaringType == enumType && m.CustomAttributes?.Any() == true);

            var attributes = new List<(T, DescriptionAttribute)>();
            foreach (var memberInfo in memberInfos)
            {
                if (predicate != null && predicate(memberInfo))
                {
                    attributes.Add(GetAttribute<T>(memberInfo));
                    break;
                }
                else if (predicate == null)
                    attributes.Add(GetAttribute<T>(memberInfo));
            }

            return attributes;
        }

        private static (T, DescriptionAttribute) GetAttribute<T>(MemberInfo memberInfo)
            where T : Enum
        {
            return ((T)Enum.Parse(typeof(T), memberInfo.Name), ((DescriptionAttribute[])memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false))[0]);
        }
    }
}
