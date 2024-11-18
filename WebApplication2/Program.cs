using WebApplication2;

namespace WebApplication2;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add controllers to the DI container
        builder.Services.AddControllers().AddXmlSerializerFormatters();
        // builder.Services.AddControllers(options =>
        // {
            // options.OutputFormatters.Add(new XmlSerializerOutputFormatterNamespace());
        // }).AddXmlSerializerFormatters();


        var app = builder.Build();

        // Map controllers
        app.MapControllers();

        app.Run();
    }
}