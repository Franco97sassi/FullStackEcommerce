//using Ecommerce.Infrastructure;
//using Ecommerce.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;
//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddInfrastructure(builder.Configuration);

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowFrontend", policy =>
//    {
//        policy
//            .WithOrigins("http://localhost:3000")
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//    });
//});

//var app = builder.Build();
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
//    //await dbContext.Database.EnsureCreatedAsync();
//    await dbContext.Database.MigrateAsync();
//    await CatalogSeed.SeedAsync(dbContext);
//}

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseCors("AllowFrontend");

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();



using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Mostrar connection string real al iniciar
var realConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"CONNECTION STRING REAL: {realConnectionString}");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();

    var connection = dbContext.Database.GetDbConnection();
    Console.WriteLine("==== DB DEBUG ====");
    Console.WriteLine($"Database: {connection.Database}");
    Console.WriteLine($"DataSource: {connection.DataSource}");
    Console.WriteLine($"ConnectionString: {connection.ConnectionString}");
    Console.WriteLine("==================");

    // await dbContext.Database.EnsureCreatedAsync();
    await dbContext.Database.MigrateAsync();
    await CatalogSeed.SeedAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Endpoint temporal de debug
app.MapGet("/debug-db", async (EcommerceDbContext dbContext) =>
{
    var connection = dbContext.Database.GetDbConnection();

    await connection.OpenAsync();

    await using var command = connection.CreateCommand();
    command.CommandText = "select current_database(), inet_server_addr(), inet_server_port();";

    await using var reader = await command.ExecuteReaderAsync();

    string? database = null;
    string? serverAddress = null;
    int? serverPort = null;

    if (await reader.ReadAsync())
    {
        database = reader.IsDBNull(0) ? null : reader.GetString(0);
        serverAddress = reader.IsDBNull(1) ? null : reader.GetValue(1)?.ToString();
        serverPort = reader.IsDBNull(2) ? null : reader.GetInt32(2);
    }

    return Results.Ok(new
    {
        database,
        serverAddress,
        serverPort,
        dataSource = connection.DataSource,
        connectionString = connection.ConnectionString
    });
});

app.Run();