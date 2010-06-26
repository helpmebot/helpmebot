#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Sets a global config option.
    /// </summary>
    internal class Set : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);


            Configuration.singleton().setOption(args[1], args[0], args[2]);
            return null;
        }
    }
}