// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IThreadedSystem.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Defines the IThreadedSystem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Threading
{
    using System;

    /// <summary>
    /// The ThreadedSystem interface.
    /// </summary>
    public interface IThreadedSystem
    {
        /// <summary>
        /// The thread fatal error event.
        /// </summary>
        event EventHandler ThreadFatalErrorEvent;

        /// <summary>
        ///   Stop all threads in this instance to allow for a clean shutdown.
        /// </summary>
        void Stop();

        /// <summary>
        ///   Register this instance of the threaded class with the global list
        /// </summary>
        void RegisterInstance();

        /// <summary>
        /// Get the status of thread(s) in this instance.
        /// </summary>
        /// <returns>
        /// The string array.
        /// </returns>
        string[] GetThreadStatus();
    }
}