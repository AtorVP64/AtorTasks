using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace AtorsTasks
{
    public record TaskRecord(List<TaskToDo> Tasks) : IEnumerable<TaskToDo>
    {
        /// <summary>
        /// The core list of tasks
        /// </summary>
        private List<TaskToDo> Tasks { get; set; } = Tasks;
        public TaskToDo this[int index] => Tasks[index];
        public static implicit operator List<TaskToDo>(TaskRecord taskRecord) => taskRecord.Tasks;

        /// <summary>
        /// Constructs an empty TaskRecord
        /// </summary>
        public TaskRecord() : this([]) { }
        /// <summary>
        /// Constructs a TaskRecord from a JSON file
        /// </summary>
        /// <param name="filepath">Filepath to the json file</param>
        /// <exception cref="JsonSerializationException">Throws an exception if the json file is not valid</exception>
        public TaskRecord(string filepath) : this([])
        {
            string file = File.ReadAllText(filepath);
            foreach (var jaTask in JArray.Parse(file))
            {
                if (jaTask["PrecedentTaskID"].Value<int?>() is not null)
                {
                    Tasks.Add(new TaskToDo(
                        jaTask["Name"]?.Value<string>() ?? throw new JsonSerializationException("Error at deserializing name"),
                        jaTask["CreationDate"]?.Value<DateTime>() ?? throw new JsonSerializationException("Error at deserializing creation date"),
                        jaTask["ID"]?.Value<int>() ?? throw new JsonSerializationException("Error at deserializing ID"),
                        jaTask["Completed"]?.Value<bool>() ?? throw new JsonSerializationException("Error at deserializing completed"),
                        jaTask["Start"].Value<DateTime?>(),
                        jaTask["End"].Value<DateTime?>(),
                        Tasks[jaTask["PrecedentTaskID"].Value<int>()]));
                }
                else
                {
                    Tasks.Add(new TaskToDo(
                        jaTask["Name"]?.Value<string>() ?? throw new JsonSerializationException("Error at deserializing name"),
                        jaTask["CreationDate"]?.Value<DateTime>() ?? throw new JsonSerializationException("Error at deserializing creation date"),
                        jaTask["ID"]?.Value<int>() ?? throw new JsonSerializationException("Error at deserializing ID"),
                        jaTask["Completed"]?.Value<bool>() ?? throw new JsonSerializationException("Error at deserializing completed"),
                        jaTask["Start"].Value<DateTime?>(),
                        jaTask["End"].Value<DateTime?>(),
                        null));
                }
            }
        }
        /// <summary>
        /// Saves the tasks in memory to a JSON file
        /// </summary>
        /// <param name="filepath">Filepath to the json file</param>
        public void Save(string filepath)
        {
            JArray ja = [];
            foreach (var task in Tasks)
            {
                ja.Add(JObject.FromObject(task));
            }
            File.WriteAllText(filepath, ja.ToString());
        }

        /// <summary>
        /// Adds a timestamp type task to the list of tasks
        /// </summary>
        /// <param name="name">The name of the task</param>
        /// <param name="timestamp">The starting timestamp of the task</param>
        public void AddTimestampTask(string name, DateTime timestamp)
        {
            Tasks.Add(new TaskToDo(timestamp, name, Tasks.Count > 0 ? Tasks.Max(task => task.ID) + 1 : 0));
        }
        /// <summary>
        /// Adds a procedural type task to the list of tasks
        /// </summary>
        /// <param name="name">The name of the task</param>
        /// <param name="precedentID">The ID of the preceding task</param>
        public void AddProceduralTask(string name, int precedentID)
        {
            Tasks.Add(new TaskToDo(name, Tasks[precedentID], Tasks.Max(task => task.ID) + 1));
        }
        /// <summary>
        /// Adds an interval type task to the list of tasks
        /// </summary>
        /// <param name="name">The name of the task</param>
        /// <param name="start">The start date of the interval of the task</param>
        /// <param name="end">The end date of the interval task</param>
        public void AddIntervalTask(string name, DateTime start, DateTime end)
        {
            Tasks.Add(new TaskToDo(start, name, end, Tasks.Max(task => task.ID) + 1));
        }
        /// <summary>
        /// Adds a due date type task to the list of tasks
        /// </summary>
        /// <param name="name">The name of the task</param>
        /// <param name="duedate">The due date of the task</param>
        public void AddDueTask(string name, DateTime duedate)
        {
            Tasks.Add(new TaskToDo(name, duedate, Tasks.Max(task => task.ID) + 1));
        }

        public void AddTaskToDo(TaskToDo task) => Tasks.Add(task);
        public IEnumerator<TaskToDo> GetEnumerator() => ((IEnumerable<TaskToDo>)Tasks).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Tasks).GetEnumerator();
    }
}
