namespace Helpmebot.CoreServices.Services.Messages
{
    public class Context
    {
        public static readonly Context Channel = new Context("channel");
        
        public string ContextType { get; }
        
        private Context(string contextType)
        {
            this.ContextType = contextType;
        }

        public override string ToString()
        {
            return $"ContextType: {this.ContextType}";
        }
    }
}