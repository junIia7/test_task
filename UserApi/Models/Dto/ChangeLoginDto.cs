using System.ComponentModel.DataAnnotations;

namespace UserApi.Models.Dto;
public class ChangeLoginDto
{
    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Login ����� ��������� ������ ��������� ����� � �����")]
    public string NewLogin { get; set; } = null!;
}
