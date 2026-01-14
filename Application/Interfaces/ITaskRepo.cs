using TODO_List.Application.DTO;
using TODO_List.Domain.Model;

namespace TODO_List.Application.Interfaces
{
    public interface ITaskRepo
    {
        Task<TaskModelDTO> AddTask(TodoModel task);
        Task<List<TaskModelDTO>> GetAllTasks();
        Task<TaskModelDTO?> SearchTaskById(int TaskID);
        Task<TaskModelDTO?> UpdateTaskById(int TaskID, TodoModel task);
        Task<bool> DeleteTaskById(int TaskID);     
    }
}
