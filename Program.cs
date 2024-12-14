using Microsoft.EntityFrameworkCore;
using netwebapi_access_control.Data;
using netwebapi_access_control.DataSP;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Adicionando DBContext do banco para acesso às tabelas
// Obs: ver a versão do MySQL Workbench (opção Help -> About)
builder.Services.AddDbContext<AccessControlContext>(options => {
    options.UseMySql(builder.Configuration.GetConnectionString("AccessControlDB"),
    Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.33"));
});

// Incluindo AddMVC do NewtonsoftJSON, após instalação do mesmo
// dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson
builder.Services.AddMvc(option => option.EnableEndpointRouting = false)
    .AddNewtonsoftJson(option => option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

// Adicionando DBContext do banco para acesso aos Stored Procedures e Views
builder.Services.AddDbContext<AccessControlContextSP>(options => {
    options.UseMySql(builder.Configuration.GetConnectionString("AccessControlDB"),
    Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.33"));
});

// Add services to the container.

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();