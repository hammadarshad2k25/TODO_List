namespace TODO_List.Domain.Model
{
    public class NH_TaskDbModel
    {
        public NH_TaskDbModel() { }
        public virtual int TaskId { get; set; }
        public virtual string TitleName { get; set; } = "";
        public virtual bool IsCompleted { get; set; }
        public virtual IList<NH_SubTaskDbModel> SubTasks { get; set; } = new List<NH_SubTaskDbModel>();
    }
}
