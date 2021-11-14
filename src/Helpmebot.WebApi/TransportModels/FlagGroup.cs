namespace Helpmebot.WebApi.TransportModels
{
    using System;

    public class FlagGroup
    {
        public FlagGroup()
        {
        }

        public FlagGroup(Model.FlagGroup import)
        {
            this.Name = import.Name;
            this.Flags = import.Flags;
            this.Mode = import.Mode;
            this.LastModified = import.LastModified;
        }

        public string Name { get; set; }
        public string Flags { get; set; }
        public string Mode { get; set; }
        public DateTime LastModified { get; set; }
    }
}