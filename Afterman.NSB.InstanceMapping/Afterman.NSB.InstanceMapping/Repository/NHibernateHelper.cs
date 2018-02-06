using System.Collections.Generic;
using System.Configuration;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace Afterman.NSB.InstanceMapping.Repository
{
    using Maps;

    public interface INHibernateHelper
    {
        void Add<T>(T obj);
        IList<T> GetAll<T>() where T : class;
    }

    public class NHibernateHelper
        : INHibernateHelper
    {
        public void Add<T>(T obj)
        {
            using (var session = NHibernateSessionHelper.OpenSession())
            {
                if (null == session) return;
                using (var transaction = session.BeginTransaction())
                {
                    session.Save(obj);
                    transaction.Commit();
                }
            }
        }

        public IList<T> GetAll<T>() where T : class
        {
            using (var session = NHibernateSessionHelper.OpenSession())
            {
                if (null == session) return new List<T>();
                using (session.BeginTransaction())
                {
                    return session.QueryOver<T>().List();
                }
            }
        }
    }

    public class NHibernateSessionHelper
    {
        private static ISessionFactory _sessionFactory;

        private static ISessionFactory SessionFactory
        {
            get
            {
                if (null == _sessionFactory)
                    InitializeSessionFactory();

                return _sessionFactory;
            }
        }

        private static void InitializeSessionFactory()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["NServiceBus/Persistence"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            _sessionFactory = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012
                    .ConnectionString(connectionString).ShowSql())
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<InstanceMappingMap>())
                .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
                .BuildSessionFactory();
        }

        public static ISession OpenSession()
        {
            return SessionFactory?.OpenSession();
        }
    }
}
