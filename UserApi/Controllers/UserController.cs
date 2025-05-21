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

    // 0) Получить пользователя по логину
    [HttpGet]
    public IActionResult GetByLogin(string login)
    {
        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "Пользователь не найден" });

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
    

    // 1) Создать пользователя
    [HttpPost("Create_User")]
    public IActionResult Create([FromHeader(Name = "UserActor"), Required] string currentUser, CreateUserDto dto)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

        if (_repo.GetByLogin(dto.Login) is not null)
            return Conflict(new { Error = "Пользователь с таким login уже существует" });

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


    // 2) Изменить данные
    [HttpPatch("Change_Profile")]
    public IActionResult UpdateProfile(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser, UpdateProfileDto dto)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || (!actor.Admin && !actor.Login.Equals(login)))
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "Пользователь не найден" });

        if (user.RevokedOn is not null)
            return BadRequest(new { Error = "Пользователь деактивирован" });

        if (dto.Name is not null) user.Name = dto.Name;
        if (dto.Gender is not null) user.Gender = dto.Gender.Value;
        if (dto.Birthday is not null) user.Birthday = dto.Birthday;

        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser;

        return NoContent();
    }


    // 3) Изменить пароль
    [HttpPatch("Change_Password")]
    public IActionResult ChangePassword(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser, ChangePasswordDto dto)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || (!actor.Admin && !actor.Login.Equals(login)))
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "Пользователь не найден" });

        if (user.RevokedOn is not null)
            return BadRequest(new { Error = "Пользователь деактивирован" });

        user.Password = dto.NewPassword;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser;

        return NoContent();
    }


    // 4) Изменить логин
    [HttpPatch("Change_Login")]
    public IActionResult ChangeLogin(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser, ChangeLoginDto dto)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || (!actor.Admin && !actor.Login.Equals(login)))
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "Пользователь не найден" });

        if (user.RevokedOn is not null)
            return BadRequest(new { Error = "Пользователь деактивирован" });

        if (_repo.GetByLogin(dto.NewLogin) is not null)
            return Conflict(new { Error = "Новый login уже занят" });

        user.Login = dto.NewLogin;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser;

        return NoContent();
    }


    // 5) Список всех активных
    [HttpGet("Active_Users")]
    public IActionResult GetAllActive([FromHeader(Name = "UserActor"), Required] string currentUser)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

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


    // 6) Запрос по логину
    [HttpGet("Request_By_Login")]
    public IActionResult GetInfo(string login, 
        [FromHeader(Name = "UserActor"), Required] string currentUser)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

        var u = _repo.GetByLogin(login);
        if (u is null)
            return NotFound(new { Error = "Пользователь не найден" });

        return Ok(new
        {
            u.Name,
            u.Gender,
            u.Birthday,
            Active = (u.RevokedOn is null)
        });
    }


    // 7) Запрос по логину и паролю (авторизация)
    [HttpGet("Authenticate")]
    public IActionResult Authenticate([FromHeader(Name = "UserActor"), Required] string currentUser,
        [FromQuery][Required] string login, [FromQuery][Required] string password)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Login.Equals(login))
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

        var u = _repo.GetByLogin(login);
        if (u is null || u.Password != password)
            return Unauthorized(new { Error = "Неверный логин или пароль" });

        if (u.RevokedOn is not null)
            return BadRequest(new { Error = "Пользователь деактивирован" });

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


    // 8) Старше чем
    [HttpGet("Older_Than")]
    public IActionResult GetOlderThan(int age, [FromHeader(Name = "UserActor"), Required] string currentUser)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

        var threshold = DateTime.UtcNow.AddYears(-age);
        var list = _repo.GetAll()
            .Where(u => u.Birthday != null && u.Birthday <= threshold)
            .Select(u => new { u.Login, u.Name, u.Birthday });

        return Ok(list);
    }


    // 9) Удаление
    [HttpDelete("Delete_User")]
    public IActionResult Delete(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser, [FromQuery] bool hard = false)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "Пользователь не найден" });

        if (hard)
        {
            _repo.Delete(user);
            return NoContent();
        }
        else
        {
            if (user.RevokedOn is not null)
                return BadRequest(new { Error = "Пользователь уже деактивирован" });

            user.RevokedOn = DateTime.UtcNow;
            user.RevokedBy = currentUser;
            return NoContent();
        }
    }


    // 10) Восстановление
    [HttpPatch("Restore_User")]
    public IActionResult Restore(string login,
        [FromHeader(Name = "UserActor"), Required] string currentUser)
    {
        var actor = _repo.GetByLogin(currentUser);
        if (actor is null || !actor.Admin)
            return Unauthorized(new { Error = "Неверные данные или недостаточно прав" });

        var user = _repo.GetByLogin(login);
        if (user is null)
            return NotFound(new { Error = "Пользователь не найден" });

        if (user.RevokedOn is null)
            return BadRequest(new { Error = "Пользователь не удалён" });

        user.RevokedOn = null;
        user.RevokedBy = null;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser;

        return NoContent();
    }
}