// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModeChanges.cs" company="Simon Walker">
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
namespace Helpmebot.ChannelServices.Model.ModeMonitoring
{
    using System.Collections.Generic;
    using System.Linq;

    public class ModeChanges
    {
        public ModeChanges()
        {
            this.Ops = new List<string>();
            this.Deops = new List<string>();
            this.Devoices = new List<string>();
            this.Voices = new List<string>();
            this.Bans = new List<string>();
            this.Unbans = new List<string>();
            this.Quiets = new List<string>();
            this.Unquiets = new List<string>();
            this.Exempts = new List<string>();
            this.Unexempts = new List<string>();
        }

        public List<string> Bans { get; private set; }

        public List<string> Deops { get; private set; }
        
        public List<string> Devoices { get; private set; }

        public List<string> Exempts { get; private set; }

        public bool? Moderated { get; set; }

        public List<string> Ops { get; private set; }

        public List<string> Quiets { get; private set; }

        public bool? ReducedModeration { get; set; }

        public bool? RegisteredOnly { get; set; }

        public List<string> Unbans { get; private set; }

        public List<string> Unexempts { get; private set; }

        public List<string> Unquiets { get; private set; }
        public List<string> Voices { get; private set; }

        public static ModeChanges FromChangeList(IEnumerable<string> rawChangeList)
        {
            var changeList = rawChangeList.ToList();
            var modeParameters = changeList.Skip(1).ToList();
            var rawChanges = changeList.First();

            var adding = true;
            var changes = new ModeChanges();

            foreach (var c in rawChanges)
            {
                switch (c)
                {
                    case '+':
                        adding = true;
                        break;
                    case '-':
                        adding = false;
                        break;
                    case 'o':
                        var op = modeParameters.First();
                        modeParameters.RemoveAt(0);
                        if (adding)
                        {
                            changes.Ops.Add(op);
                        }
                        else
                        {
                            changes.Deops.Add(op);
                        }

                        break;
                    case 'v':
                        var voice = modeParameters.First();
                        modeParameters.RemoveAt(0);
                        if (adding)
                        {
                            changes.Voices.Add(voice);
                        }
                        else
                        {
                            changes.Devoices.Add(voice);
                        }
                        break;
                    case 'b':
                        var ban = modeParameters.First();
                        modeParameters.RemoveAt(0);
                        if (adding)
                        {
                            changes.Bans.Add(ban);
                        }
                        else
                        {
                            changes.Unbans.Add(ban);
                        }

                        break;
                    case 'q':
                        var quiet = modeParameters.First();
                        modeParameters.RemoveAt(0);
                        if (adding)
                        {
                            changes.Quiets.Add(quiet);
                        }
                        else
                        {
                            changes.Unquiets.Add(quiet);
                        }

                        break;
                    case 'e':
                        var exempt = modeParameters.First();
                        modeParameters.RemoveAt(0);
                        if (adding)
                        {
                            changes.Exempts.Add(exempt);
                        }
                        else
                        {
                            changes.Unexempts.Add(exempt);
                        }

                        break;
                    case 'z':
                        changes.ReducedModeration = adding;
                        break;
                    case 'm':
                        changes.Moderated = adding;
                        break;
                    case 'r':
                        changes.RegisteredOnly = adding;
                        break;
                    case 'f':
                    case 'j':
                    case 'l':
                    case 'k':
                        if (adding)
                        {
                            modeParameters.RemoveAt(0);
                        }

                        break;
                }
            }

            return changes;
        }

        public bool IsEmpty()
        {
            if (this.Bans.Any()) return false;
            if (this.Deops.Any()) return false;
            if (this.Exempts.Any()) return false;
            if (this.Ops.Any()) return false;
            if (this.Quiets.Any()) return false;
            if (this.Unbans.Any()) return false;
            if (this.Unexempts.Any()) return false;
            if (this.Unquiets.Any()) return false;
            if (this.Voices.Any()) return false;
            if (this.Devoices.Any()) return false;

            if (this.Moderated.HasValue) return false;
            if (this.ReducedModeration.HasValue) return false;
            if (this.RegisteredOnly.HasValue) return false;

            return true;
        }
    }
}