using Microsoft.OpenApi.Models;
using UserApi.Models;
using UserApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });
});

// Репозиторий
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// Контроллеры
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "User API V1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Контроллеры
app.MapControllers();

// Создание Admin
var repo = app.Services.GetRequiredService<IUserRepository>();
if (repo.GetByLogin("Admin") is null)
{
    repo.Create(new User
    {
        Login = "Admin",
        Password = "Admin123",
        Name = "СуперАдмин",
        Gender = 2,
        Birthday = null,
        Admin = true,
        CreatedBy = "System"
    });
}

app.Run();
