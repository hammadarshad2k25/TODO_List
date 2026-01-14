namespace TODO_List.Application.DTO
{
    public class UpdateTaskRequest
    {

        public string tname { get; set; } = "";
        public bool tisCompleted { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public List<SubTaskModelDTO> subTasks { get; set; }
    }
}
