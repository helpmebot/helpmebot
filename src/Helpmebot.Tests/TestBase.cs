﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestBase.cs" company="Helpmebot Development Team">
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
//   The base class for all the unit tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests
{
    using System;
    using System.Collections.Generic;

    using Castle.Core.Logging;
    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// The base class for all the unit tests.
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected Mock<ILogger> Logger { get; set; }

        /// <summary>
        /// The common setup.
        /// </summary>
        [OneTimeSetUp]
        public void CommonSetup()
        {
            this.Logger = new Mock<ILogger>();
            this.Logger.Setup(x => x.CreateChildLogger(It.IsAny<string>())).Returns(this.Logger.Object);

            this.Logger.Setup(x => x.Fatal(It.IsAny<string>())).Callback(() => Assert.Fail("Logger recorded fatal error."));
            this.Logger.Setup(x => x.Fatal(It.IsAny<string>(), It.IsAny<Exception>())).Callback(() => Assert.Fail("Logger recorded fatal error."));
            this.Logger.Setup(x => x.FatalFormat(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Callback(() => Assert.Fail("Logger recorded fatal error."));
            this.Logger.Setup(x => x.FatalFormat(It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Callback(() => Assert.Fail("Logger recorded fatal error.")); 
            
            this.Logger.Setup(x => x.Error(It.IsAny<string>())).Callback(() => Assert.Fail("Logger recorded error."));
            this.Logger.Setup(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>())).Callback(() => Assert.Fail("Logger recorded error."));
            this.Logger.Setup(x => x.ErrorFormat(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Callback(() => Assert.Fail("Logger recorded error."));
            this.Logger.Setup(x => x.ErrorFormat(It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Callback(() => Assert.Fail("Logger recorded error."));

            this.LocalSetup();
        }

        /// <summary>
        /// The local setup.
        /// </summary>
        public virtual void LocalSetup()
        {
        }
    }
}
