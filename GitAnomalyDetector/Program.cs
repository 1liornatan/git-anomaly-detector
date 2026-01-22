using GitAnomalyDetector.Notifications;
using GitAnomalyDetector.Notifications.Implementations;
using GitAnomalyDetector.Services;
using GitAnomalyDetector.Services.AnomalyDetection;

var builder = WebApplication.CreateBuilder(args);

var webHostUrl = builder.Configuration["WebHost:Url"] ?? "http://127.0.0.1:3000";
builder.WebHost.UseUrls(webHostUrl);

builder.Services.AddControllers();
builder.Services.AddTransient<IEventService, EventService>();

builder.Services.AddTransient<IAnomalyDetectionAction, UnusualRepositoryAccessAction>();
builder.Services.AddTransient<IAnomalyDetectionAction, UnusualPushTimeAction>();
builder.Services.AddTransient<IAnomalyDetectionAction, UnusualTeamNameAction>();

builder.Services.AddTransient<INotificationAction, ConsoleNotificationAction>();

var app = builder.Build();

app.MapControllers();

app.Run();
