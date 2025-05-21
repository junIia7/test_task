using System.ComponentModel.DataAnnotations;

namespace UserApi.Models.Dto;
public class ChangePasswordDto
{
    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "������ ����� ��������� ������ ��������� ����� � �����")]
    public string NewPassword { get; set; } = null!;
}