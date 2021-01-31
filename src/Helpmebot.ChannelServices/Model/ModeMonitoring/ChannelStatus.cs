// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelStatus.cs" company="Simon Walker">
//   Copyright (c) 2016 Simon Walker
//   -
//   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
//   documentation files (the "Software"), to deal in the Software without restriction, including without limitation
//   the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
//   to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above
//   copyright notice and this permission notice shall be included in all copies or substantial portions of the
//   Software.
//   -
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
//   THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
//   CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
//   IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Helpmebot.ChannelServices.Model.ModeMonitoring
{
    using System;

    internal class ChannelStatus
    {
        public ChannelStatus()
        {
            this.Bans = new List<string>();
            this.Quiets = new List<string>();
            this.Exempts = new List<string>();

            this.LastOverrideTime = DateTime.MinValue;
        }

        public bool BotOpsRequested { get; set; }
        public bool BotOpsHeld { get; set; }
        public bool ReducedModeration { get; set; }
        public bool Moderated { get; set; }
        public bool RegisteredOnly { get; set; }
        public List<string> Bans { get; private set; }
        public List<string> Quiets { get; private set; }
        public List<string> Exempts { get; private set; }

        public bool BanListDownloaded { get; set; }
        public bool QuietListDownloaded { get; set; }
        public bool ExemptListDownloaded { get; set; }

        public bool FirstTimeSyncComplete { get; set; }

        public DateTime LastOverrideTime { get; set; }
    }
}