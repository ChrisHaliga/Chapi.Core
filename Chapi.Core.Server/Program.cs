using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Services;
using Chapi.Api.Services.ApiServices;
using Chapi.Api.Services.DatabaseItemServices;
using Chapi.Api.Utilities;
using Chapi.Core.Server.Models.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var auth0ConfigData = builder.Configuration.GetSection("Auth0ConfigData").Get<Auth0ConfigDataDto>();
if (auth0ConfigData == null) throw new InvalidOperationException("Auth0ConfigData data is missing or invalid.");
builder.Services.AddSingleton(auth0ConfigData.ToValidated());

// Add services to the container.
var cosmosConfigData = builder.Configuration.GetSection("CosmosConfigData").Get<CosmosConfigDataDto>();
if(cosmosConfigData == null) throw new InvalidOperationException("CosmosConfig data is missing or invalid.");
builder.Services.AddSingleton(cosmosConfigData.ToValidated());

CrudConfigDataDto<UserWithId>.AddSingleton(builder, "UsersConfigData");
CrudConfigDataDto<GroupWithId>.AddSingleton(builder, "GroupsConfigData");
CrudConfigDataDto<ApplicationWithId>.AddSingleton(builder, "ApplicationsConfigData");


var auth0ClientSecret = builder.Configuration.GetValue<string>("Auth0ClientSecret");

builder.Services.AddSingleton(new RuntimeInfo(builder.Environment.IsDevelopment()));

builder.Services.AddTransient<ICacheService, CacheService>();

builder.Services.AddTransient<DatabaseUserService>();
builder.Services.AddTransient<DatabaseGroupService>();
builder.Services.AddTransient<DatabaseApplicationService>();

builder.Services.AddTransient<UserApiService>();
builder.Services.AddTransient<GroupApiService>();
builder.Services.AddTransient<ApplicationApiService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddControllers();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{auth0ConfigData.Domain}/";
        options.Audience = auth0ConfigData.Audience;
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Auth0 in Swagger was basically exactly as outlined in this article: https://www.nredko.com/articles/auth0-webapi-swagger
    // - The only thing missing was go to Advanced Settings > Grant Types and enable Authorization Code
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        BearerFormat = "JWT",
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://{auth0ConfigData.Domain}/authorize?audience={auth0ConfigData.Audience}"),
                TokenUrl = new Uri($"https://{auth0ConfigData.Domain}/oauth/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenId" }
                },
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
          {
              new OpenApiSecurityScheme
              {
                  Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
              },
              new[] { "openid" }
          }
      });
});


BuildValidated.Validate();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId(auth0ConfigData.ClientId);
        c.OAuthClientSecret(auth0ClientSecret);
        c.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
