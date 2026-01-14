namespace TODO_List.Application.DTO
{
    public class NH_TaskRequestDTO
    {
        public string tname { get; set; }
        public bool tisCompleted { get; set; }
        public List<NH_SubTaskRequestDTO> subTasks { get; set; }
    }
}
