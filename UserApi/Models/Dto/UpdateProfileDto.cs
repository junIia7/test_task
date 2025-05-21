using System.ComponentModel.DataAnnotations;

namespace UserApi.Models.Dto;
public class UpdateProfileDto
{
    [RegularExpression("^[A-Za-z�-��-���]+$", ErrorMessage = "Name ����� ��������� ������ ��������� � ������� �����")]
    public string? Name { get; set; }

    [Range(0, 2)]
    public int? Gender { get; set; }

    public DateTime? Birthday { get; set; }
}