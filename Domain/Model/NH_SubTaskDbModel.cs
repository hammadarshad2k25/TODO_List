namespace TODO_List.Domain.Model
{
    public class NH_SubTaskDbModel
    {
        public NH_SubTaskDbModel() { }
        public virtual int SubTaskId { get; set; }
        public virtual string SubTaskName { get; set; } = "";
        public virtual int TaskId { get; set; }
        public virtual NH_TaskDbModel Task { get; set; }
    }
}
