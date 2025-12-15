using System.Text.Json.Serialization;
using Stella.MinimalApi.Extensions;
using Stella.MinimalApi.Sample.Data;
using Stella.MinimalApi.Sample.Endpoints;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddEndpoints(new GeneratedEndpointDiscovery());
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapEndpoints();


app.Run();

