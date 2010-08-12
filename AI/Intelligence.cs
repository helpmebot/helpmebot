#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.AI
{
    internal class Intelligence
    {
        private static Intelligence _singleton;

        public static Intelligence singleton()
        {
            return _singleton ?? ( _singleton = new Intelligence( ) );
        }

        protected Intelligence()
        {
        }

        public string respond(string input)
        {
            string[] tokens = input.Split(' ');

            string[] hello = {"hi", "hey", "heya", "hello"};
            const string helloResponses = "cmdSayHi1";
            string[] morning = {"morning", "good morning"};
            const string morningResponses = "cmdSayHiMorning";
            string[] afternoon = {"afternoon", "good afternoon"};
            const string afternoonResponses = "cmdSayHiAfternoon";
            string[] evening = {"evening", "good evening"};
            const string eveningResponses = "cmdSayHiEvening";

            foreach (string word in hello)
            {
                if ( tokens.Length <= 1 ) continue;
                if (tokens[0] == word && tokens[1] == Helpmebot6.irc.ircNickname)
                {
                    return helloResponses;
                }
            }

            foreach (string word in morning)
            {
                if (tokens.Length > 1)
                {
                    if (tokens[0] == word && tokens[1] == Helpmebot6.irc.ircNickname)
                    {
                        return morningResponses;
                    }
                }
            }

            foreach (string word in afternoon)
            {
                if (tokens.Length > 1)
                {
                    if (tokens[0] == word && tokens[1] == Helpmebot6.irc.ircNickname)
                    {
                        return afternoonResponses;
                    }
                }
            }

            foreach (string word in evening)
            {
                if (tokens.Length > 1)
                {
                    if (tokens[0] == word && tokens[1] == Helpmebot6.irc.ircNickname)
                    {
                        return eveningResponses;
                    }
                }
            }

            if (input.Contains("Ayn Rand"))
                return "Ayn Rand? The eminent 20th-century Russian-American philosopher?";

            return "";




        }
    }
}