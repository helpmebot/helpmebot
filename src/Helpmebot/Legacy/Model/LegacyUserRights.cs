namespace Helpmebot.Legacy.Model
{
    /// <summary>
    ///     The user rights.
    /// </summary>
    public enum LegacyUserRights
    {
        /// <summary>
        ///     The developer.
        /// </summary>
        Developer = 3, 

        /// <summary>
        ///     The super user.
        /// </summary>
        Superuser = 2, 

        /// <summary>
        ///     The advanced.
        /// </summary>
        Advanced = 1, 

        /// <summary>
        ///     The normal.
        /// </summary>
        Normal = 0, 

        /// <summary>
        ///     The semi-ignored.
        /// </summary>
        Semiignored = -1, 

        /// <summary>
        ///     The ignored.
        /// </summary>
        Ignored = -2
    }
}