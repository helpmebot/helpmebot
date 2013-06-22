namespace helpmebot6.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
            {
                throw new ArgumentNullException("format");
            }

            return source.Aggregate(format, (current, keyValuePair) => current.Replace("{" + keyValuePair.Key + "}", keyValuePair.Value.ToString()));
        }
    }
}
