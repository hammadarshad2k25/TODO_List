using StoreTasks = System.Collections.Generic.List<TODO_List.Domain.Model.TodoModel>;
using TODO_List.Domain.Model;

namespace TODO_List.Infrastructure.Storage
{
    public class Tstore
    {
        public StoreTasks ST = new StoreTasks();
        public TodoModel TM = new TodoModel("", false, 0);
    }
}
