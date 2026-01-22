using GitAnomalyDetector.Services;
using GitAnomalyDetector.Services.AnomalyDetection;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:3000");
builder.Services.AddControllers();
builder.Services.AddTransient<IEventService, EventService>();
builder.Services.AddTransient<IAnomalyDetectionAction, UnusualRepositoryAccessAction>();
builder.Services.AddTransient<IAnomalyDetectionAction, UnusualPushTimeAction>();
builder.Services.AddTransient<IAnomalyDetectionAction, UnusualTeamNameAction>();

var app = builder.Build();

app.MapControllers();

app.Run();
