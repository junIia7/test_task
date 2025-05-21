using UserApi.Models;

namespace UserApi.Repositories;
public interface IUserRepository
{
    IEnumerable<User> GetAll();
    User? GetByLogin(string login);
    void Create(User user);
    void Update(User user);
    void Delete(User user);
}
