using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [Required]
        public string Status { get; set; }

        public bool isFavorite { get; set; }

        public string ImageUrl { get; set; }
    }

    public class UploadFileResponse
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
    }

}
