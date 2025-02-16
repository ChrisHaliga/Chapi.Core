using Chapi.Api.Middleware;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Services;
using Chapi.Api.Services.ApiServices;
using Chapi.Api.Services.DatabaseItemServices;
using Chapi.Api.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var cosmosConfigData = builder.Configuration.GetSection("CosmosConfigData").Get<CosmosConfigDataDto>();
if(cosmosConfigData == null) throw new InvalidOperationException("CosmosConfig data is missing or invalid.");
builder.Services.AddSingleton(cosmosConfigData.ToValidated());

CrudConfigDataDto<UserWithId>.AddSingleton(builder, "UsersConfigData");
CrudConfigDataDto<GroupWithId>.AddSingleton(builder, "GroupsConfigData");
CrudConfigDataDto<ApplicationWithId>.AddSingleton(builder, "ApplicationsConfigData");


var authorizationKey = builder.Configuration.GetValue<string>("AuthorizationKey");
if (string.IsNullOrEmpty(authorizationKey)) throw new InvalidOperationException("AuthorizationKey data is missing or invalid.");
builder.Services.AddSingleton(new ApiKeyAuthorization(authorizationKey));
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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<ApiKeyAuthorizationFilter>();
});

BuildValidated.Validate();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
