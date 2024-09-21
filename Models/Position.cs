using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class Position
    {
        public Position()
        {
            Employees = new List<Employee>();
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "Назва")]
        [Required(ErrorMessage = "Поле не повинно бути  порожнім")]
        [StringLength(30, ErrorMessage = "Назва не має перевищувати 30 символів")]
        public string Name { get; set; } = null!;

        public virtual ICollection<Employee> Employees { get; set; }
    }
}
