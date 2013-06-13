using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace helpmebot6.ExtensionMethods
{
    public static class FormatWithExtension
    {

        public static string FormatWith(this string format, object source, string[] data)
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();

            foreach (var prop in source.GetType().GetProperties())
            {
                dict.Add(prop.Name, prop.GetValue(source, null));
            }

            for (int i = 0; i < data.Length; i++)
            {
                dict.Add(i.ToString(), data[i]);
            }

            return FormatWith(format, dict);
        }

        public static string FormatWith(this string format, IDictionary<string,object> source)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            Regex r = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
              RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            List<object> values = new List<object>();
            string rewrittenFormat = r.Replace(format, delegate(Match m)
                {
                    Group startGroup = m.Groups["start"];
                    Group propertyGroup = m.Groups["property"];
                    Group formatGroup = m.Groups["format"];
                    Group endGroup = m.Groups["end"];


                    try
                    {
                        values.Add(source[propertyGroup.Value]);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        values.Add("{" + propertyGroup.Value + "|missing!}");
                    }

                    return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value
                           + new string('}', endGroup.Captures.Count);
                });

            return string.Format(rewrittenFormat, values.ToArray());
        }
    }
}
