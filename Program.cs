using System.Text.Json;
using Enricher.Middleware;
using Enricher.Models;

var builder = WebApplication.CreateBuilder(args);

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

app.UseMiddleware<Enricher.Middleware.ResponseEnrichmentMiddleware>();

app.MapGet("/products", [ResponseType(typeof(Product))]() =>
{
    return Results.Ok(new Product { Id = 17, Id_Text_Name = 4 });
})
.WithOpenApi();

app.Run();
