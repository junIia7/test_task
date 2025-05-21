using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UserApi.Models;
using UserApi.Models.Dto;
using UserApi.Repositories;

namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;

    public UsersController(IUserRepository repo)
    {
        _repo = repo;
    }

    // 0) �������� ������������ �� ������
    [HttpGet]
    public IActionResult GetByLogin(string login)
    {
        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "������������ �� ������" });

        var response = new UserResponseDto
        {
            Guid = user.Guid,
            Login = user.Login,
            Name = user.Name,
            Gender = user.Gender,
            Birthday = user.Birthday,
            Admin = user.Admin,
            CreatedOn = user.CreatedOn,
            CreatedBy = user.CreatedBy,
            RevokedOn = user.RevokedOn
        };

        return Ok(response);
    }
    

    // 1) ������� ������������
    [HttpPost("Create_User")]
    public IActionResult Create([FromHeader(Name = "UserActor"), Required] string currentUser, CreateUserDto dto)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        if (_repo.GetByLogin(dto.Login) is not null)
            return Conflict(new { Error = "������������ � ����� login ��� ����������" });

        var user = new User
        {
            Login = dto.Login,
            Password = dto.Password,
            Name = dto.Name,
            Gender = dto.Gender,
            Birthday = dto.Birthday,
            Admin = dto.Admin,
            CreatedBy = "Admin"
        };

        _repo.Create(user);

        var response = new UserResponseDto
        {
            Guid = user.Guid,
            Login = user.Login,
            Name = user.Name,
            Gender = user.Gender,
            Birthday = user.Birthday,
            Admin = user.Admin,
            CreatedOn = user.CreatedOn,
            CreatedBy = user.CreatedBy,
            RevokedOn = user.RevokedOn
        };

        return CreatedAtAction(
            nameof(GetByLogin),
            new { login = response.Login },
            response
        );
    }


    // 2) �������� ������
    [HttpPatch("Change_Profile")]
    public IActionResult UpdateProfile(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser, UpdateProfileDto dto)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || (!actor.Admin && !actor.Login.Equals(login)))
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "������������ �� ������" });

        if (user.RevokedOn is not null)
            return BadRequest(new { Error = "������������ �������������" });

        if (dto.Name is not null) user.Name = dto.Name;
        if (dto.Gender is not null) user.Gender = dto.Gender.Value;
        if (dto.Birthday is not null) user.Birthday = dto.Birthday;

        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser;

        return NoContent();
    }


    // 3) �������� ������
    [HttpPatch("Change_Password")]
    public IActionResult ChangePassword(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser, ChangePasswordDto dto)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || (!actor.Admin && !actor.Login.Equals(login)))
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "������������ �� ������" });

        if (user.RevokedOn is not null)
            return BadRequest(new { Error = "������������ �������������" });

        user.Password = dto.NewPassword;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser;

        return NoContent();
    }


    // 4) �������� �����
    [HttpPatch("Change_Login")]
    public IActionResult ChangeLogin(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser, ChangeLoginDto dto)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || (!actor.Admin && !actor.Login.Equals(login)))
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "������������ �� ������" });

        if (user.RevokedOn is not null)
            return BadRequest(new { Error = "������������ �������������" });

        if (_repo.GetByLogin(dto.NewLogin) is not null)
            return Conflict(new { Error = "����� login ��� �����" });

        user.Login = dto.NewLogin;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser;

        return NoContent();
    }


    // 5) ������ ���� ��������
    [HttpGet("Active_Users")]
    public IActionResult GetAllActive([FromHeader(Name = "UserActor"), Required] string currentUser)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        var list = _repo.GetAll()
            .Where(u => u.RevokedOn is null)
            .OrderBy(u => u.CreatedOn)
            .Select(u => new UserResponseDto
            {
                Guid = u.Guid,
                Login = u.Login,
                Name = u.Name,
                Gender = u.Gender,
                Birthday = u.Birthday,
                Admin = u.Admin,
                CreatedOn = u.CreatedOn,
                CreatedBy = u.CreatedBy,
                RevokedOn = u.RevokedOn
            });

        return Ok(list);
    }


    // 6) ������ �� ������
    [HttpGet("Request_By_Login")]
    public IActionResult GetInfo(string login, 
        [FromHeader(Name = "UserActor"), Required] string currentUser)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        var u = _repo.GetByLogin(login);
        if (u is null)
            return NotFound(new { Error = "������������ �� ������" });

        return Ok(new
        {
            u.Name,
            u.Gender,
            u.Birthday,
            Active = (u.RevokedOn is null)
        });
    }


    // 7) ������ �� ������ � ������ (�����������)
    [HttpGet("Authenticate")]
    public IActionResult Authenticate([FromHeader(Name = "UserActor"), Required] string currentUser,
        [FromQuery][Required] string login, [FromQuery][Required] string password)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Login.Equals(login))
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        var u = _repo.GetByLogin(login);
        if (u is null || u.Password != password)
            return Unauthorized(new { Error = "�������� ����� ��� ������" });

        if (u.RevokedOn is not null)
            return BadRequest(new { Error = "������������ �������������" });

        return Ok(new
        {
            u.Guid,
            u.Login,
            u.Name,
            u.Gender,
            u.Birthday,
            u.Admin,
            Active = true
        });
    }


    // 8) ������ ���
    [HttpGet("Older_Than")]
    public IActionResult GetOlderThan(int age, [FromHeader(Name = "UserActor"), Required] string currentUser)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        var threshold = DateTime.UtcNow.AddYears(-age);
        var list = _repo.GetAll()
            .Where(u => u.Birthday != null && u.Birthday <= threshold)
            .Select(u => new { u.Login, u.Name, u.Birthday });

        return Ok(list);
    }


    // 9) ��������
    [HttpDelete("Delete_User")]
    public IActionResult Delete(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser, [FromQuery] bool hard = false)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "������������ �� ������" });

        if (hard)
        {
            _repo.Delete(user);
            return NoContent();
        }
        else
        {
            if (user.RevokedOn is not null)
                return BadRequest(new { Error = "������������ ��� �������������" });

            user.RevokedOn = DateTime.UtcNow;
            user.RevokedBy = currentUser;
            return NoContent();
        }
    }


    // 10) ��������������
    [HttpPatch("Restore_User")]
    public IActionResult Restore(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "�������� ������ ��� ������������ ����" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "������������ �� ������" });

        if (user.RevokedOn is null)
            return BadRequest(new { Error = "������������ �� �����" });

        user.RevokedOn = null;
        user.RevokedBy = null;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser;

        return NoContent();
    }
}