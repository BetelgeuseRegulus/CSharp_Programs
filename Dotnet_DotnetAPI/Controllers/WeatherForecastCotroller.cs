using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controller;

// [] 表示特性(Attribute),给当前类(方法,属性)添加"元数据"(metadata), 启用特性的额外行为.
[ApiController] // 开启 Web API 的智能行为：自动模型验证、自动 400 返回、改进参数绑定
[Route("[Controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly string[] _summaries = new[]
     {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpGet("", Name = "GetWeatherForecast")] // 声明这是一个 GET 接口 + 配置路由
    public IEnumerable<WeatherForecast> GetFiveDayForcast()
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            _summaries[Random.Shared.Next(_summaries.Length)]
        ))
        .ToArray();

        return forecast;
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
