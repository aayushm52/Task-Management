using System;

public class Class1
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; } // "ToDo", "InProgress", "Done"
        public bool IsFavorited { get; set; }
        public string ImageUrl { get; set; } // URL to the attached image
    }
}
