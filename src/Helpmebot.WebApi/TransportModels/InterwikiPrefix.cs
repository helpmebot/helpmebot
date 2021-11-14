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
            this.Url = prefix.Url;
            this.ImportedAs = prefix.ImportedAs;
            this.AbsentFromLastImport = prefix.AbsentFromLastImport;
            this.CreatedSinceLast = prefix.CreatedSinceLast;
        }
        
        public string Prefix { get; set; }
        public string Url { get; set; }
        public string ImportedAs { get; set; }
        public bool AbsentFromLastImport { get;set;}
        public bool CreatedSinceLast { get;set;}
    }
}