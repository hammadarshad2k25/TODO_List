using RepoDb.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TODO_List.Domain.Entities
{
    [Map("Tasks")]
    public class TaskDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TaskId { get; set; }  
        public string TitleName { get; set; } = "";
        public bool IsCompleted { get; set; }
        public List<SubTaskDbModel> SubTasks { get; set; }
    }
}
