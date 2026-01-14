namespace TODO_List.Application.DTO
{
    public class CosmosModelDTO
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public bool isCompleted { get; set; }
        public DateTime createdDate { get; set; }
    }
}
