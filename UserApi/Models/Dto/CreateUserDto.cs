using System.ComponentModel.DataAnnotations;

namespace UserApi.Models.Dto;
public class CreateUserDto
{
    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Login может содержать только латинские буквы и цифры")]
    public string Login { get; set; } = null!;

    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Password может содержать только латинские буквы и цифры")]
    public string Password { get; set; } = null!;

    [Required]
    [RegularExpression("^[A-Za-zА-Яа-яЁё]+$", ErrorMessage = "Name может содержать только латинские и русские буквы")]
    public string Name { get; set; } = null!;

    [Range(0, 2)]
    public int Gender { get; set; }

    public DateTime? Birthday { get; set; }

    public bool Admin { get; set; }
}