namespace TODO_List.Infrastructure.NhibernateConfig
{
    public class AuditLogModel
    {
        public virtual int AuditId { get; set; }
        public virtual string EntityName { get; set; } = "";
        public virtual string Action { get; set; } = ""; 
        public virtual int EntityId { get; set; }
        public virtual DateTime ChangeTime { get; set; }
        public virtual string OldValue { get; set; } = "";
        public virtual string NewValue { get; set; } = "";
    }
}
