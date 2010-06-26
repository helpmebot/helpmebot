#region Usings

using System;
using System.IO;
using System.Net;
using System.Reflection;

#endregion

namespace helpmebot6
{
    internal class IsGd
    {
        public static Uri shorten(Uri longUrl)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            DAL.Select q = new DAL.Select("suc_shorturl");
            q.setFrom("shorturlcache");
            q.addWhere(new DAL.WhereConds("suc_fullurl", longUrl.ToString()));
            string cachelookup = DAL.singleton().executeScalarSelect(q);

            if (cachelookup == "")
            {
                HttpWebRequest wrq = (HttpWebRequest) WebRequest.Create("http://is.gd/api.php?longurl=" + longUrl);
                wrq.UserAgent = Configuration.singleton().retrieveGlobalStringOption("useragent");
                HttpWebResponse wrs = (HttpWebResponse) wrq.GetResponse();
                if (wrs.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader sr = new StreamReader(wrs.GetResponseStream());
                    string shorturl = sr.ReadLine();
                    DAL.singleton().insert("shorturlcache", "", longUrl.ToString(), shorturl);
                    return new Uri(shorturl);
                }
                return null;
            }
            return new Uri(cachelookup);
        }
    }
}