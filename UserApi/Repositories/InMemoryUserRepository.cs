using UserApi.Models;

namespace UserApi.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public IEnumerable<User> GetAll() => _users;

    public User? GetByLogin(string login) =>
        _users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));

    public void Create(User user) => _users.Add(user);

    public void Update(User user) { }

    public void Delete(User user) => _users.Remove(user);
}