namespace Helpmebot.CoreServices.Model
{
    using Helpmebot.CoreServices.Attributes;

    public class Flags
    {
        [FlagHelp("Access to bot information", "Provides information about the bot itself")]
        public const string BotInfo = "B";
        
        [FlagHelp("Access to general information", "Allows use of the information commands, which provide information which is publicly available")]
        public const string Info = "I";
        
        [FlagHelp("Access to WP:ACC commands", "Allows access to the WP:ACC commands, including !accstatus and !acccount")]
        public const string Acc = "a";
        
        [FlagHelp("Access to fun commands", "Allows use of the fun commands, such as !trout")]
        public const string Fun = "F";
        
        [FlagHelp("Access to channel-local configuration", "Allows changing the configuration of the bot within a single channel only")]
        public const string LocalConfiguration = "c";
        
        [FlagHelp("Access to sensitive data commands", "Allows access to commands which provide data which is slightly more sensitive, such as !whois, !fetchall, and !curl")]
        public const string Protected = "P";
        
        [FlagHelp("Access to ACL modification commands", "Allows changing the access control lists, and which flags users have")]
        public const string AccessControl = "A";
        
        [FlagHelp("Access to bot management commands", "Allows management of the bot")]
        public const string BotManagement = "M";
        
        [FlagHelp("Access to brain commands", "Allows learning and forgetting new learnt commands")]
        public const string Brain = "b";
        
        [FlagHelp("Access to global configuration commands", "Allows changing the configuration of the bot in any channel it is in")]
        public const string Configuration = "C";
        
        [FlagHelp("Enable fun commands", "Allows re-enabling the fun commands in a channel where they have been disabled.")]
        public const string Uncurl = "U";
        
        [FlagHelp("Access to channel operator commands", "Allows using commands which perform channel operator actions")]
        public const string ChanOp = "o";
        
        [FlagHelp("Access to Owner-only commands", "Allows access to commands which are highly sensitive, such as !raw")]
        public const string Owner = "O";

        [FlagHelp("Standard commands", "Commands anyone can use")]
        public const string Standard = "S";
    }
}