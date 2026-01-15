namespace TODO_List.Domain.Model
{
    public class TodoModel(string TName, bool TIsCompleted, int TId)
    {
        public string Tname { get; set; } = TName;
        public bool TisCompleted { get; set; } = TIsCompleted;
        public int Tid { get; set; } = TId;
        public string GetTname() => Tname;
        public bool GetTisCompleted() => TisCompleted;
        public int GetTid() => Tid;
    }
}
