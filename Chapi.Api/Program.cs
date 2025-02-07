using Chapi.Api.Middleware;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Services;
using Chapi.Api.Services.CrudServices;
using Chapi.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var cosmosConfigData = builder.Configuration.GetSection("CosmosConfigData").Get<CosmosConfigDataDto>();
if(cosmosConfigData == null) throw new InvalidOperationException("CosmosConfig data is missing or invalid.");
builder.Services.AddSingleton(cosmosConfigData.ToValidated());

CrudConfigDataDto<User>.AddSingleton(builder, "UsersConfigData");
CrudConfigDataDto<Group>.AddSingleton(builder, "GroupsConfigData");

var authorizationKey = builder.Configuration.GetValue<string>("AuthorizationKey");
if (string.IsNullOrEmpty(authorizationKey)) throw new InvalidOperationException("AuthorizationKey data is missing or invalid.");
builder.Services.AddSingleton(new ApiKeyAuthorization(authorizationKey));

builder.Services.AddTransient<IDatabaseService, DatabaseService>();
builder.Services.AddTransient<DatabaseService>();
builder.Services.AddTransient<CacheService>();

builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<GroupService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<ApiKeyAuthorizationFilter>();
});

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
