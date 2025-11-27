using DotnetAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS = Cross-origin Resource shared,跨源资源共享
builder.Services.AddCors((option) =>
{
    option.AddPolicy("DevCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
    option.AddPolicy("ProdCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("http://myProductionSite.com")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

builder.Services.AddScoped<IUserRepository, UserRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors"); // 开发环境 CORS 策略，通常允许任意域访问 API，方便调试
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors"); // 生产环境 CORS 策略，只允许特定域访问 API，保证安全
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();


