using NHibernate;

namespace TODO_List.Infrastructure.NhibernateConfig
{
    public class AuditInterceptors : EmptyInterceptor
    {
        public NHibernate.ISession? _session;
        public void SetSession(NHibernate.ISession session)
        {
            _session = session;
        }
        public override bool OnSave(object entity, object id, object[] state, string[] propertynames, NHibernate.Type.IType[] types)
        {
            SaveAuditLog(entity, id, "INSERT", null, state);
            return true;
        }
        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, NHibernate.Type.IType[] types)
        {
            SaveAuditLog(entity, id, "UPDATE", previousState, currentState);
            return true;
        }
        public override void OnDelete(object entity, object id, object[] state, string[] propertyNames, NHibernate.Type.IType[] types)
        {
            SaveAuditLog(entity, id, "DELETE", state, null);
        }
        private void SaveAuditLog(object entity, object id, string action, object? oldstate, object? newstate)
        {
            if (_session == null) 
                return;
            var log = new AuditLogModel
            {
                EntityName = entity.GetType().Name,
                Action = action,
                EntityId = (int)id,
                ChangeTime = DateTime.UtcNow,
                OldValue = oldstate is object[] prev ? string.Join(", ", prev.Select(x => x?.ToString())) : null,
                NewValue = newstate is object[] curr ? string.Join(", ", curr.Select(x => x?.ToString())) : null
            };
            using var auditSession = _session.SessionFactory.OpenSession();
            auditSession.Save(log);
            auditSession.Flush();
        }
    }
}
