using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class Employee
    {
        public Employee()
        {
            Tasks = new List<Task>();
        }
        [Key]
        public int Id { get; set; }

        [Display(Name = "Ім'я")]
        [Required(ErrorMessage = "Поле не повинно бути  порожнім")]
        [StringLength(50, ErrorMessage = "Ім'я не має перевищувати 50 символів")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯІіЇїЄє']+$", ErrorMessage = "В імені дозволені лише літери та апостроф")]
        public string Name { get; set; } = null!;

        [Display(Name = "Прізвище")]
        [Required(ErrorMessage = "Поле не повинно бути  порожнім")]
        [StringLength(50, ErrorMessage = "Прізвище не має перевищувати 50 символів")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯІіЇїЄє']+$", ErrorMessage = "У прізвищі дозволені лише літери та апостроф")]
        public string Surname { get; set; } = null!;

        public string FullName { get; set; } = null!;

        [Display(Name = "Адреса електронної пошти")]
        [Required(ErrorMessage = "Поле не повинно бути  порожнім")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Потрібний формат email: example@example.com")]
        public string Email { get; set; } = null!;

        [Display(Name = "Номер телефону")]
        [Required(ErrorMessage = "Поле не повинно бути  порожнім")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Потрібний формат номеру телефону: +380XXXXXXXXX")]
        public string PhoneNumber { get; set; } = null!;

        [Display(Name = "Позиція")]
        public int PositionId { get; set; }
        [Display(Name = "Позиція")]
        public virtual Position? Position { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }
    }
}
