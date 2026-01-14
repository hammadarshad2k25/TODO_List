namespace TODO_List.Application.DTO
{
    public class NH_TaskResponseDTO
    {
        public int TaskId { get; set; }
        public string TitleName { get; set; }
        public bool IsCompleted { get; set; }
        public List<NH_SubTaskResponseDTO> SubTasks { get; set; }
    }
}
