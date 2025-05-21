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

// �����������
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// �����������
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

// �����������
app.MapControllers();

// �������� Admin
var repo = app.Services.GetRequiredService<IUserRepository>();
if (repo.GetByLogin("Admin") is null)
{
    repo.Create(new User
    {
        Login = "Admin",
        Password = "Admin123",
        Name = "����������",
        Gender = 2,
        Birthday = null,
        Admin = true,
        CreatedBy = "System"
    });
}

app.Run();
