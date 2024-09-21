using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class Status
    {
        public Status()
        {
            Tasks = new List<Models.Task>();
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "Назва")]
        [Required(ErrorMessage = "Поле не повинно бути  порожнім")]
        [StringLength(30, ErrorMessage = "Назва не має перевищувати 30 символів")]
        public string Name { get; set; } = null!;

        public virtual ICollection<Models.Task> Tasks { get; set; }
    }
}
