namespace TODO_List.Application.DTO
{
    public class AddTaskRequest
    {
        public int tid { get; set; }
        public string tname { get; set; } = "";
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public bool tisCompleted { get; set; }
        public List<SubTaskModelDTO> subTasks { get; set; }
    }
}
