using System.ComponentModel.DataAnnotations;

namespace UserApi.Models.Dto;
public class ChangeLoginDto
{
    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Login может содержать только латинские буквы и цифры")]
    public string NewLogin { get; set; } = null!;
}
