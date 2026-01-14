using System.Text.Json.Serialization;

namespace TODO_List.Domain.Entities
{
    public class TodoTaskCosmos
    {
        public string id { get; set; }
        public string UserId { get; set; }  
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
