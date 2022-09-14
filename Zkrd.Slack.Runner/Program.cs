using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Zkrd.Slack.Core;
using Zkrd.Slack.FooBar;
using Zkrd.Slack.Help;
using Zkrd.Slack.Weather;
using Zkrd.Slack.WebApiDemo;

WebApplicationBuilder builder = WebApplication.CreateBuilder();
builder.Services.AddFastEndpoints();
builder.Services.AddSlackBackgroundService(builder.Configuration);
builder.Services.AddSlackFoobar();
builder.Services.AddSlackBotHelp();
builder.Services.AddHelloWorldController();
builder.Services.AddWeatherService();
builder.Services.AddSwaggerDoc();

WebApplication app = builder.Build();

app.UseAuthorization();
app.UseFastEndpoints();
app.UseOpenApi();
app.UseSwaggerUi3(settings =>
{
   settings.ConfigureDefaults(uiSettings =>
   {
      uiSettings.Path = string.Empty;
   } );
});

app.Run();
