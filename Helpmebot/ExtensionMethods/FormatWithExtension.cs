namespace helpmebot6.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class FormatWithExtension
    {
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
