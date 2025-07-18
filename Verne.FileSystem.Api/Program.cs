using Microsoft.EntityFrameworkCore;
using Verne.FileSystem.Core.Interfaces;
using Verne.FileSystem.Infrastructure.Persistence;
using Verne.FileSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FileSystemDbContext>(options => 
    options.UseInMemoryDatabase("FileSystemDb"));

builder.Services.AddScoped<IFileSystemService, FileSystemService>();
builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();
