using RepoDb.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TODO_List.Domain.Entities
{
    [Map("SubTaskDbModel")]
    public class SubTaskDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SubTaskId { get; set; }
        public string SubTaskName { get; set; } = "";
        public int TaskId { get; set; }
        public TaskDbModel Task { get; set; }
    }
}
