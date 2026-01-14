namespace TODO_List.Application.DTO
{
    public class TaskModelDTO
    {
        public string Tname { get; set; }
        public bool TisCompleted { get; set; }
        public int Tid { get; set; }
        public int SubTaskId { get; set; }
        public string SubTaskName { get; set; } = "";
        public List<SubTaskModelDTO> subTasks { get; set; }
    }
}
