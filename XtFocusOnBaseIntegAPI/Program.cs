using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.Newtonsoft;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Filters;
using XtFocusOnBaseIntegAPI.Model;
using static XtFocusOnBaseIntegAPI.Model.Account;
using static XtFocusOnBaseIntegAPI.Model.PO;
using log4net.Config;
using log4net;

var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();

//configure log4net
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OnBase To Focus Integration", Version = "v1" });
    c.IncludeXmlComments(xmlPath);
    c.ExampleFilters();
    // This ensures nested types are uniquely identified
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<ItemExample>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<AccountExample>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<POExample>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<PV.PVExample>();
builder.Services.AddSwaggerGenNewtonsoftSupport();


var app = builder.Build();

//// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() ||app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
app.UseSwaggerUI(c =>
{
 
    c.SwaggerEndpoint("v1/swagger.json", "OnBaseToFocus V1");
    c.DefaultModelsExpandDepth(-1); // Hides the entire "Schemas" section
    c.DefaultModelExpandDepth(0);   // Prevents individual model expansion
    c.DefaultModelRendering(ModelRendering.Example); // Shows example instead of schema
});


}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
