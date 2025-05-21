namespace UserApi.Models.Dto;
public class UserResponseDto
{
    public Guid Guid { get; set; }
    public string Login { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool Admin { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime? RevokedOn { get; set; }
}
