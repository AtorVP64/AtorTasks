using Newtonsoft.Json;

namespace AtorsTasks
{
    public class TaskToDo
    {
        /// <summary>
        /// Name of the task
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Creation date of the task<br/>
        /// Default set to the current date
        /// </summary>
        public DateTime CreationDate { get; init; }
        /// <summary>
        /// Serial ID of the task<br/>
        /// Should be unique to each task in the record
        /// </summary>
        public int ID { get; init; }
        /// <summary>
        /// Whether the task is completed or not<br/>
        /// Only settable in a constructor and with the Complete() method
        /// </summary>
        public bool Completed { get; private set; }

        /// <summary>
        /// Whether the task is late for completion or not<br/>
        /// Automatically calculated based on the end date or the precedent task's late status
        /// </summary>
        [JsonIgnore]
        public bool Late
        {
            get
            {
                if (End != null)
                {
                    return End < DateTime.Now && !Completed;
                }
                else if (precedentTask != null)
                {
                    return precedentTask.Late;
                }
                else return false;
            }
        }

        private DateTime? start;
        /// <summary>
        /// Start date of the task<br/>
        /// Null if the task is a Due Task or a Followup Task
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Throws an exception if the set would result in an invalid task type
        /// </exception>
        public DateTime? Start
        {
            get => start; 

            set
            {
                if (value is not null)
                {
                    //We wanna set it to a date:
                    if (precedentTask is not null)
                    {
                        //If it's a followup task, it can't have a start date, throws error
                        throw new ArgumentException("This is a followup task, giving it a start or end date is not allowed");
                    }
                    else
                    {
                        //If it's not a followup task, no problem...
                        if (end is not null && (end < start || start < DateTime.Now))
                        {
                            //...unless the basic logic of start and end dates is wrong
                            throw new ArgumentException("End must be after start and in the future");
                        }
                        start = value;
                    }
                }
                else
                {
                    //We wanna set it to null
                    if (end is null)
                    {
                        //If end is null
                        if (precedentTask is not null)
                        {
                            //If it's not a followup task, then error, a task needs to have a start or end otherwise.
                            throw new ArgumentException("Both a start and end need to exist if it's not a followup task");
                        }
                        else
                        {
                            //If it's a followup task, start is null and we wanna set start to null;
                            start = value;
                        }
                    }
                    else
                    {
                        //If end is not null, no problem
                        start = value;
                    }
                }
            }
        }

        private DateTime? end;
        /// <summary>
        /// End date of the task<br/>
        /// Null if the task is a Timestamp Task or a Followup Task
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Throws an exception if the set would result in an invalid task type
        /// </exception>
        public DateTime? End
        {
            get => end;

            set
            {
                if (value is not null)
                {
                    //We wanna set it to a date:
                    if (precedentTask is not null)
                    {
                        //If it's a followup task, it can't have a start date, throws error
                        throw new ArgumentException("This is a followup task, giving it a start or end date is not allowed");
                    }
                    else
                    {
                        //If it's not a followup task, no problem...
                        if (start is not null && (end < start || end < DateTime.Now))
                        {
                            //...unless the basic logic of start and end dates is wrong
                            throw new ArgumentException("End must be after start and in the future");
                        }
                        end = value;
                    }
                }
                else
                {
                    //We wanna set it to null
                    if (start is null)
                    {
                        //If start is null
                        if (precedentTask is not null)
                        {
                            //If it's not a followup task, then error, a task needs to have a start or end otherwise.
                            throw new ArgumentException("Both a start and end need to exist if it's not a followup task");
                        }
                        else
                        {
                            //If it's a followup task, end is null and we wanna set end to null;
                            end = value;
                        }
                    }
                    else
                    {
                        //If start is not null, no problem
                        end = value;
                    }
                }
            }
        }

        /// <summary>
        /// The ID of the preceding task<br/>
        /// Used for storing the precedent task by reference in JSON<br/>
        /// Is null if the task isn't a Followup Task
        /// </summary>
        public int? PrecedentTaskID 
        { 
            get
            {
                return precedentTask?.ID;
            }
        }

        /// <summary>
        /// The reference to the preceding task<br/>
        /// Is null if the task isn't a Followup Task
        /// </summary>
        [JsonIgnore]
        public TaskToDo? precedentTask;

        /// <summary>
        /// Only used for JSON deserialization
        /// </summary>
        public TaskToDo(string name, DateTime creationDate, int ID, bool completed, DateTime? start, DateTime? end, TaskToDo? task)
        {
            Name = name;
            CreationDate = DateTime.Now;
            this.ID = ID;
            Completed = completed;
            this.start = start;
            this.end = end;
            precedentTask = task;
        }

        /// <summary>
        /// The real constructor of the class, only called from the other constrctors unless it's reading from JSON
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Throws an exception if dates are invalid - start must be before end and both have to be before the current date
        /// </exception>
        private TaskToDo(DateTime? start, string name, DateTime? end, int ID, TaskToDo? task)
        {
            if (start != null && start <= DateTime.Now)
            {
                throw new ArgumentException("Start must be in the future");
            }
            if (end != null && end <= DateTime.Now)
            {
                throw new ArgumentException("End must be in the future");
            }
            if (start != null && end != null && start >= end)
            {
                throw new ArgumentException("Start must be before end");
            }
            Name = name;
            this.start = start;
            this.end = end;
            CreationDate = DateTime.Now;
            this.ID = ID;
            precedentTask = task;
        }
        /// <summary>
        /// Constructor for an Interval Task, it has both a start and end date<br/>
        /// Sets precedentTask to null
        /// </summary>
        /// <param name="start">The start date of the task</param>
        /// <param name="name">The name of the task</param>
        /// <param name="end">The end date of the task</param>
        /// <param name="ID">Serial ID</param>
        /// <exception cref="ArgumentException">
        /// Throws an exception if dates are invalid - start must be before end and both have to be before the current date
        /// </exception>
        public TaskToDo(DateTime start, string name, DateTime end, int ID) : this(start, name, end, ID, null) { }
        /// <summary>
        /// Constructor for a Timestamp Task, only has a start date<br/>
        /// Sets precedentTask and end date to null
        /// </summary>
        /// <param name="timestamp">Timestamp about when to start the task</param>
        /// <param name="name">The name of the task</param>
        /// <param name="ID">Serial ID</param>
        /// <exception cref="ArgumentException">
        /// Throws an exception if dates are invalid - start must be before end and both have to be before the current date
        /// </exception>
        public TaskToDo(DateTime timestamp, string name, int ID) : this(timestamp, name, null, ID, null) { }
        /// <summary>
        /// Constructor for a Due Date Task, only has a due date<br/>
        /// Sets precedentTask and start date to null
        /// </summary>
        /// <param name="name">The name of the task</param>
        /// <param name="duedate">Due date of the task to finish it by</param>
        /// <param name="ID">Serial ID</param>
        /// <exception cref="ArgumentException">
        /// Throws an exception if dates are invalid - start must be before end and both have to be before the current date
        /// </exception>
        public TaskToDo(string name, DateTime duedate, int ID) : this(null, name, duedate, ID, null) { }
        /// <summary>
        /// Constructor for a Followup Task - a task that procedurally follows another task<br/>
        /// Sets start and end dates to null
        /// </summary>
        /// <param name="name">The name of the task</param>
        /// <param name="task">The referenced task which this follows</param>
        /// <param name="ID">Serial ID</param>
        public TaskToDo(string name, TaskToDo task, int ID) : this(null, name, null, ID, task) { }

        public override string ToString() =>
            $"ID: {ID},\n" +
            $"Name: {Name},\n" +
            $"CreationDate: {CreationDate},\n" +
            $"Completed?: {(Completed ? "Yes" : "No")},\n" +
            $"Late?: {(Late ? "Yes" : "No")},\n" +
            $"Start date: {Start},\n" +
            $"End date: {End},\n" +
            $"Follows: {precedentTask?.Name}";

        /// <summary>
        /// Completes the task<br/>
        /// Can not complete the task if the start date has not yet passed<br/>
        /// Can not complete the task if the task has a precedent task which is not completed yet
        /// </summary>
        public void Complete()
        {
            if (precedentTask is not null)
            {
                if (precedentTask.Completed)
                {
                    Completed = true;
                }
            }
            else
            {
                if (Start is not null && Start < DateTime.Now)
                {
                    Completed = true;
                }
            }
        }
    }
}
