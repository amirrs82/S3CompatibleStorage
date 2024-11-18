using WebApplication2;

namespace WebApplication2;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddXmlSerializerFormatters();

        var app = builder.Build();

        app.MapControllers();

        app.Run();
    }
}