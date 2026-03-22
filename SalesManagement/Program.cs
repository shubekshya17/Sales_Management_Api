using Microsoft.EntityFrameworkCore;
using SalesManagement.Configuration;
using SalesManagement.Data;
using SalesManagement.Services.Implementation;
using SalesManagement.Services.Interface;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ISalesCollection, SalesCollectionService>();
builder.Services.AddScoped<ISalesDetail, SalesDetailService>();
builder.Services.AddScoped<ICategoryRange, CategoryRangeService>();
builder.Services.AddScoped<ISalesDetailReport, SalesDetailReportService>();
builder.Services.AddScoped<IKOTService, KOTService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
MapsterConfig.RegisterMappings();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
