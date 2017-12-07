namespace Website.Managers
{
    public class Task
    {
        public enum TaskType { IndexRepository }
        public TaskType Type { get; set; }
    }

    public class TaskManager
    {
        public static TaskManager Instance = new TaskManager();
        private TaskManager() { }
    }
}
