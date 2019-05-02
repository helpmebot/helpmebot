namespace Helpmebot.Legacy.Transitional
{
    using System;
    using Castle.Core.Logging;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;
    using Microsoft.Practices.ServiceLocation;
    using MySql.Data.MySqlClient;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class LegacyAccessService : ILegacyAccessService
    {
        private readonly ILogger logger;
        private readonly ILegacyDatabase legacyDatabase;

        public LegacyAccessService(ILogger logger)
        {
            this.logger = logger;

            // FIXME: service locator - legacy database
            this.legacyDatabase = ServiceLocator.Current.GetInstance<ILegacyDatabase>();
        }

        public bool IsAllowed(LegacyUserRights required, IUser user)
        {
            var actual = this.GetLegacyUserRights(user);

            if (actual == LegacyUserRights.Ignored)
            {
                return false;
            }

            return actual >= required;
        }

        public LegacyUserRights GetLegacyUserRights(IUser user)
        {
            try
            {
                var command =
                    new MySqlCommand(
                        "SELECT user_accesslevel FROM user WHERE @nick LIKE user_nickname AND @user LIKE user_username AND @host LIKE user_hostname ORDER BY user_accesslevel ASC;");
                command.Parameters.AddWithValue("@nick", user.Nickname);
                command.Parameters.AddWithValue("@user", user.Username);
                command.Parameters.AddWithValue("@host", user.Hostname);

                string retrievedAccessLevel = this.legacyDatabase.ExecuteScalarSelect(command);

                if (string.IsNullOrEmpty(retrievedAccessLevel))
                {
                    retrievedAccessLevel = "Normal";
                }

                var actual = (LegacyUserRights) Enum.Parse(typeof(LegacyUserRights), retrievedAccessLevel);
                return actual;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.Message, ex);
            }

            return LegacyUserRights.Normal;
        }
    }
}