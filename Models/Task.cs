using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class Task
    {
        public Task()
        {
            Employees = new List<Employee>();
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

        [Display(Name = "Термін виконання")]
        [Required(ErrorMessage = "Поле не повинно бути  порожнім")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Файл з подробицями завдання")]
        public string? FileUrl { get; set; } 

        [Display(Name = "Проєкт")]
        public int ProjectId { get; set; }
        [Display(Name = "Проєкт")]
        public virtual Project? Project { get; set; }

        [Display(Name = "Статус")]
        public int StatusId { get; set; }
        [Display(Name = "Статус")]
        public virtual Status? Status { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
    }
}
