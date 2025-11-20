var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ConfigurableGoogleSearchDemo>("google-search-demo");

builder.AddProject<Projects.TaskChatDemo>("task-chat-demo");

builder.AddProject<Projects.TaskDemoAPI>("task-demo-api");

builder.Build().Run();
