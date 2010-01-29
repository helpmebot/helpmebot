using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Threading
{
    public interface IThreadedSystem
    {
        /// <summary>
        /// Stop all threads in this instance to allow for a clean shutdown.
        /// </summary>
        void Stop( );

        /// <summary>
        /// Register this instance of the threaded class with the global list
        /// </summary>
        void RegisterInstance( );

        /// <summary>
        /// Get the status of thread(s) in this instance.
        /// </summary>
        /// <returns></returns>
        string[ ] getThreadStatus( );
    }
}
