using System.Text;
using AutoMapper;
using FluentEmail.MailKitSmtp;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShittyOne.Data;
using ShittyOne.Entities;
using ShittyOne.Hangfire;
using ShittyOne.Hangfire.Jobs;
using ShittyOne.Mappings;
using ShittyOne.Models;
using ShittyOne.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default"), o => o.CommandTimeout(300)));


builder.Services.AddIdentity<User, IdentityRole<Guid>>(opts =>
    {
        opts.Password.RequiredLength = 8;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequiredUniqueChars = 1;
        opts.User.RequireUniqueEmail = true;
        // opts.Tokens.PasswordResetTokenProvider = ResetPasswordTokenProvider.ProviderKey;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
//.AddTokenProvider<ResetPasswordTokenProvider>(ResetPasswordTokenProvider.ProviderKey);

var jwtOptions = builder.Configuration.GetSection("JwtOptions");

builder.Services.Configure<JwtOptions>(opts =>
{
    opts.SecretKey = jwtOptions["SecretKey"]!;
    opts.Audience = jwtOptions["Audience"]!;
    opts.Issuer = jwtOptions["Issuer"]!;
    opts.EpiresIn = jwtOptions["ExpiresIn"]!;
    opts.RrefreshLifetime = TimeSpan.FromDays(double.Parse(jwtOptions["RefreshLifetime"]!));
    opts.ValidFor = TimeSpan.FromHours(Convert.ToDouble(jwtOptions["ExpireDays"]));
});

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
    {
        opts.SaveToken = true;
        opts.RequireHttpsMetadata = false;
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = jwtOptions["Audience"],
            ValidateIssuer = true,
            ValidIssuer = jwtOptions["Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                jwtOptions["SecretKey"]!)),
            ValidateLifetime = true,
            SaveSigninToken = true,
            ClockSkew = TimeSpan.Zero
        };
    });
var smtpEmailOptions = builder.Configuration.GetSection("SmtpEmailOptions");

builder.Services.AddFluentEmail(smtpEmailOptions["User"])
    .AddRazorRenderer()
    .AddMailKitSender(smtpEmailOptions.Get<SmtpClientOptions>());

builder.Services.Configure<ImapEmailOptions>(c => builder.Configuration.GetSection("ImapEmailOptions").Bind(c));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddJobManager()
    .AddrecurringJob<FilesCleanUpJob>()
    .AddrecurringJob<EmailSurveyJob>();


builder.Services.AddSingleton(provider => new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new MappingProfile(provider.GetService<IHttpContextAccessor>()!));
}).CreateMapper());

builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddVersionedApiExplorer(o => o.SubstituteApiVersionInUrl = true);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("1.0", new OpenApiInfo { Title = "Gazprom Corporate Surveys API", Version = "1.0" });
    c.CustomOperationIds(e => e.ActionDescriptor.RouteValues["action"]);

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Description = "Type 'Bearer [JWT]'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

    var basePath = AppContext.BaseDirectory;
    var xmlPath = Path.Combine(basePath, "Shitty.xml");

    // c.IncludeXmlComments(xmlPath);
});

builder.Services.AddControllers()
    // TODO newtonsoft json in 2023??
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        o.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

builder.Services.AddSwaggerGenNewtonsoftSupport();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "";
        c.SwaggerEndpoint("/swagger/1.0/swagger.json", "Gazprom Corporate Surveys API v1.0");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers",
            "Origin, X-Requested-With, Content-Type, Accept");
    }
});

app.UseRouting();
app.UseCors(builder => builder.SetIsOriginAllowed(o => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();

app.StartRecurringJobs();
app.UseHangfireDashboard("/Dashboard",
    new DashboardOptions { Authorization = new IDashboardAuthorizationFilter[] { new HangfireAuthFilter() } });

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
    endpoints.MapControllers();
});

await SeedDB.Initialize(app.Services.CreateScope().ServiceProvider);

app.Run();