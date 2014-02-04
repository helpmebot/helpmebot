// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistenceFacility.cs" company="Helpmebot Development Team">
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
//   Defines the PersistenceFacility type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Startup.Facilities
{
    using Castle.MicroKernel.Facilities;
    using Castle.MicroKernel.Registration;

    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;

    using Helpmebot.Configuration;
    using Helpmebot.Persistence;
    using Helpmebot.Persistence.Interfaces;

    using NHibernate;
    using NHibernate.Cfg;

    /// <summary>
    /// The persistence facility.
    /// </summary>
    public class PersistenceFacility : AbstractFacility
    {
        /// <summary>
        /// Initialise the persistence facility.
        /// </summary>
        protected override void Init()
        {
            var config = this.BuildDatabaseConfiguration();

            this.Kernel.Register(Component.For<ISessionFactoryProvider>().ImplementedBy<SessionFactoryProvider>());
            this.Kernel.Register(Component.For<Configuration>().Instance(config));

            this.Kernel.Register(
                Component.For<ISessionFactory>()
                    .UsingFactoryMethod(k => k.Resolve<ISessionFactoryProvider>().SessionFactory),
                Component.For<ISession>().UsingFactoryMethod(k => k.Resolve<ISessionFactory>().OpenSession()).LifestyleTransient());
        }

        /// <summary>
        /// Builds the database configuration.
        /// </summary>
        /// <returns>
        /// The <see cref="Configuration"/>.
        /// </returns>
        private Configuration BuildDatabaseConfiguration()
        {
            return
                Fluently.Configure()
                    .Database(this.SetupDatabase)
                    .Mappings(a => a.FluentMappings.AddFromAssemblyOf<EntityBase>())
                    .ExposeConfiguration(this.ConfigurePersistence)
                    .BuildConfiguration();
        }

        /// <summary>
        /// The configure persistence.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        private void ConfigurePersistence(Configuration config)
        {
            // This is a fix for a wierd exception.
            // {"Column 'ReservedWord' does not belong to table ReservedWords."}
            // http://stackoverflow.com/questions/1061128/mysql-nhibernate-how-fix-the-error-column-reservedword-does-not-belong-to
            config.Properties.Add("hbm2ddl.keywords", "none");
        }

        /// <summary>
        /// The setup database.
        /// </summary>
        /// <returns>
        /// The <see cref="IPersistenceConfigurer"/>.
        /// </returns>
        private IPersistenceConfigurer SetupDatabase()
        {
            var connectionString = this.Kernel.Resolve<IConfigurationHelper>().PrivateConfiguration.ConnectionString;

            return MySQLConfiguration.Standard.ConnectionString(connectionString.ConnectionString);
        }
    }
}
