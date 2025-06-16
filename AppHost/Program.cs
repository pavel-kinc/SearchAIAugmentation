var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DemoApp>("promptenhancer");

builder.Build().Run();
