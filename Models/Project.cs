using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class Project
    {

        public Project()
        {
            Tasks = new List<Models.Task>();
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "Назва")]
        [Required(ErrorMessage = "Поле не повинно бути  порожнім")]
        [StringLength(50, ErrorMessage = "Назва не має перевищувати 50 символів")]
        public string Name { get; set; } = null!;

        [Display(Name = "Опис")]
        [Required(ErrorMessage = "Поле не повинно бути  порожнім")]
        public string Description { get; set; } = null!;

        public virtual ICollection<Models.Task> Tasks { get; set; }
    }
}
