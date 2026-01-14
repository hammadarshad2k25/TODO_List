namespace TODO_List.Application.DTO
{
    public class DynamoModelDTO
    {
        public string Tname { get; set; }
        public bool TisCompleted { get; set; }
        public int Tid { get; set; }
        public List<SubTaskModelDTO> subTasks { get; set; }
    }
}
