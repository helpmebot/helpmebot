/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/

#region Usings

using System;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

#endregion

namespace helpmebot6
{
    /// <summary>
    ///   Talks to the API for Nubio squared.
    /// </summary>
    public class NubioApi
    {
        private readonly Uri _apiUri; // http://stable.toolserver.org/nubio/api.php

        public NubioApi(Uri apiUri)
        {
            _apiUri = apiUri;
        }

        public string fetchFaqText(int id)
        {
            try
            {
                XmlTextReader xtr =
                    new XmlTextReader(HttpRequest.get(_apiUri + "?format=xml&noparse=true&action=fetch&id=" + id))
                        {
                            WhitespaceHandling = WhitespaceHandling.None
                        };

                xtr.Read();
                try
                {
                    xtr.Read();
                }
                catch (WebException)
                {
                }
                string title = "";
                string text =  "";
                while (xtr.Read())
                {
                    if (xtr.NodeType == XmlNodeType.Element)
                    {
                        if (xtr.Name == "rev_text")
                            text =
                                HttpUtility.HtmlDecode(Regex.Replace(
                                    xtr.ReadElementContentAsString().Replace("\\", ""), "<(.|\n)*?>", ""));
                        if (xtr.Name == "page_title")
                            title =
                                HttpUtility.HtmlDecode(Regex.Replace(
                                    xtr.ReadElementContentAsString().Replace("\\", ""), "<(.|\n)*?>", ""));
                    }
                }

                if (text != "" && title != "")
                {
                    return title + ": " + text;
                }
                return Configuration.singleton().getMessage("fetchFaqTextNotFound", id.ToString());
            }
            catch (Exception ex)
            {
                GlobalFunctions.errorLog(ex);
            }
            return null;
        }

        public string searchFaq(string searchTerm)
        {
            try
            {
                XmlTextReader xtr =
                    new XmlTextReader(
                        HttpRequest.get(_apiUri + "?format=xml&action=search&noparse=true&query=" + searchTerm))
                        {
                            WhitespaceHandling = WhitespaceHandling.None
                        };

                xtr.Read();
                try
                {
                    xtr.Read();
                }
                catch (WebException)
                {
                }

                string title;
                string text = title = "";

                while (xtr.Read())
                {
                    if (xtr.Name == "page_title" && xtr.NodeType == XmlNodeType.Element)
                    {
                        title =
                            HttpUtility.HtmlDecode(Regex.Replace(xtr.ReadElementContentAsString().Replace("\\", ""),
                                                                 "<(.|\n)*?>", ""));
                        text =
                            HttpUtility.HtmlDecode(Regex.Replace(xtr.ReadString().Replace("\\", ""), "<(.|\n)*?>", ""));
                    }
                }
                if (text != "" && title != "")
                {
                    return title + ": " + text;
                }
                return Configuration.singleton().getMessage("fetchFaqTextNotFound", searchTerm);
            }
            catch (Exception ex)
            {
                GlobalFunctions.errorLog(ex);
            }
            return null;
        }

        public string viewLink(int id)
        {
            return _apiUri.ToString().Replace("api.php", "index.php?id=" + id);
        }
    }
}