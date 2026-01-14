using NHibernate;
using NHibernate.Cfg;
using System.Reflection;
using System.Xml;

namespace TODO_List.Infrastructure.NhibernateConfig
{
    public static class NH_Helper
    {
        private static ISessionFactory? _fact;
        public static ISessionFactory fact
        {
            get
            {
                if (_fact == null)
                {
                    var cfg = new Configuration();
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "TODO_List.Infrastructure.NhibernateConfig.hibernate.cfg.xml";
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null)
                        throw new FileNotFoundException(
                            $"Embedded resource not found: '{resourceName}'. " +
                            "Run Assembly.GetManifestResourceNames() to list available resources.");
                    using var xmlReader = XmlReader.Create(stream);
                    cfg.Configure(xmlReader);
                    var mapper = new NHibernate.Mapping.ByCode.ModelMapper();
                    mapper.AddMapping<AuditLogMaping>();
                    var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
                    cfg.AddMapping(mapping);
                    _fact = cfg.SetInterceptor(new AuditInterceptors()).BuildSessionFactory();
                }
                return _fact;
            }
        }
        public static ISessionFactory BuildSessionFactory()
        {
            var cfg = new Configuration();
            cfg.Configure();
            return cfg.SetInterceptor(new AuditInterceptors()).BuildSessionFactory();
        }
    }
}
