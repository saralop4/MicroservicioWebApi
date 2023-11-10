
using DataSignosVitales.Data;
using DataSignosVitales.Entities.NotaEnfermeriaModels;
using DataSignosVitales.Interfaces;
using LogicaSignosVitales.Exceptions;
using LogicaSignosVitales.Interfaces;
using LogicaSignosVitales.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;



var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomModelStateValidationFilter>(); 
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwaggerUI",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Signos Vitales", Version = "v1" });

    // Configuracion para la autenticacion con JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:key"])),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });


builder.Services.AddAuthorization(options => {
    options.AddPolicy("SuperAdmin", policy => policy.RequireClaim("AdminType", "Admin"));
});

builder.Services.AddSqlServer<NotaEnfermeriaDbContext>(builder.Configuration.GetConnectionString("SignosVitales"));

builder.Services.AddScoped<INotaEnfermeriaDbContext, NotaEnfermeriaDbContext>();

builder.Services.AddScoped<INotaEnfermeriaService, NotaEnfemeriaService>();

builder.Services.AddScoped<ICensoService , CensoService>(); 

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    // Use CORS
    app.UseCors("AllowSwaggerUI");

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contabilidad");
        c.OAuthClientId("swagger");
        c.OAuthClientSecret("secret");
        c.OAuthRealm("realm");
        c.OAuthAppName("Mi API");
    });
}

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 401)
    {
        var result = JsonSerializer.Serialize(new { message = "No se a logeado para realizar este proceso." });
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
    if (context.Response.StatusCode == 403)
    {
        var result = JsonSerializer.Serialize(new { message = "No tienes permiso para realizar esta acción." });
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
    if (context.Response.StatusCode == 500)
    {
        var result = JsonSerializer.Serialize(new { message = "Ah ocurrido un error inesperado en el servidor,porfavor contactar al administrador del sistema." });
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
});





app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
