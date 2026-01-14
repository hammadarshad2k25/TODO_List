using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace TODO_List.Infrastructure.NhibernateConfig
{
    public class AuditLogMaping : ClassMapping<AuditLogModel>
    {
        public AuditLogMaping() 
        {
            Table("AuditLogs");
            Id(x => x.AuditId, m => {
                m.Column("AuditId");
                m.Generator(Generators.Identity);
            });
            Property(x => x.EntityName);
            Property(x => x.Action);
            Property(x => x.EntityId);
            Property(x => x.ChangeTime);
            Property(x => x.OldValue);
            Property(x => x.NewValue);
        }
    }
}
