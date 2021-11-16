namespace Helpmebot.Configuration
{
    using System.Collections.Generic;

    public class FileMessageStore
    {
        public int Format { get; set; }
        public Dictionary<string,List<List<string>>> Dataset { get; set; }
    }
}