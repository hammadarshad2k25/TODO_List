using Amazon.DynamoDBv2.DataModel;

namespace TODO_List.Domain.Entities
{
    [DynamoDBTable("TodoTasks")]
    public class TodoTaskDynamo
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }
        [DynamoDBRangeKey]
        public string TaskId { get; set; }
        [DynamoDBProperty]
        public string Title { get; set; }
        [DynamoDBProperty]
        public string Description { get; set; }
        [DynamoDBProperty]
        public bool IsCompleted { get; set; }
        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; }
    }
}