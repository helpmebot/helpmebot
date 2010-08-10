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

using System.Reflection;

#endregion

namespace helpmebot6
{
    public class WordLearner
    {
        public static bool learn(string word, string phrase)
        {
            return learn(word, phrase, false);
        }

        public static bool learn(string word, string phrase, bool action)
        {
            DAL.Select q = new DAL.Select("COUNT(*)");
            q.setFrom("keywords");
            q.addLimit(1, 0);
            q.addWhere(new DAL.WhereConds("keyword_name", word));


            if (DAL.singleton().executeScalarSelect(q) != "0")
                return false;

            DAL.singleton().insert("keywords", "", word, phrase, (action ? "1" : "0 "));
            return true;
        }

        public static RemeberedWord remember(string word)
        {
            DAL.Select q = new DAL.Select("keyword_action");
            q.setFrom("keywords");
            q.addWhere(new DAL.WhereConds("keyword_name", word));

            string action = DAL.singleton().executeScalarSelect(q);
            q = new DAL.Select("keyword_response");
            q.setFrom("keywords");
            q.addWhere(new DAL.WhereConds("keyword_name", word));
            string result = DAL.singleton().executeScalarSelect(q);

            RemeberedWord rW = new RemeberedWord
                                   {
                                       action = ( action == "1" ? true : false ),
                                       phrase = result
                                   };

            return rW;
        }

        public static bool forget(string word)
        {
            DAL.Select q = new DAL.Select("COUNT(*)");
            q.setFrom("keywords");
            q.addWhere(new DAL.WhereConds("keyword_name", word));

            if (DAL.singleton().executeScalarSelect(q) == "0")
                return false;

            DAL.singleton().delete("keywords", 0, new DAL.WhereConds("keyword_name", word));
            return true;
        }

        public struct RemeberedWord
        {
            public string phrase;
            public bool action;
        }
    }
}