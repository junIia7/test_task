using System.ComponentModel.DataAnnotations;

namespace UserApi.Models.Dto;
public class UpdateProfileDto
{
    [RegularExpression("^[A-Za-zА-Яа-яЁё]+$", ErrorMessage = "Name может содержать только латинские и русские буквы")]
    public string? Name { get; set; }

    [Range(0, 2)]
    public int? Gender { get; set; }

    public DateTime? Birthday { get; set; }
}