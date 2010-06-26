#region Usings

using System.Diagnostics;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the current version of the bot.
    /// </summary>
    internal class Version : GenericCommand
    {
        public string version
        {
            get { return "6.0"; }
        }

        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (GlobalFunctions.isInArray("@svn", args) != -1)
                return new CommandResponseHandler(getVersionString());
            return new CommandResponseHandler(this.version);
        }

        public string getVersionString()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);


            string rev = Process.Start("svnversion").StandardOutput.ReadLine();

            string versionString = version + "-r" + rev;

            return versionString;
        }
    }
}