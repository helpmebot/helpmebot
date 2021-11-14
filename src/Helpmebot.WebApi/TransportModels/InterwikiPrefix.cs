namespace Helpmebot.WebApi.TransportModels
{
    using System.Text;

    public class InterwikiPrefix
    {
        public InterwikiPrefix()
        {
        }

        public InterwikiPrefix(Model.InterwikiPrefix prefix)
        {
            this.Prefix = prefix.Prefix;
            this.Url = Encoding.UTF8.GetString(prefix.Url);
        }
        
        public string Prefix { get; set; }
        public string Url { get; set; }
    }
}