using System.ComponentModel.DataAnnotations;

namespace UserApi.Models.Dto;
public class ChangePasswordDto
{
    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Пароль может содержать только латинские буквы и цифры")]
    public string NewPassword { get; set; } = null!;
}