/*
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SmartHome.Plugins;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var apiKey = config["OpenAI:ApiKey"];

var builder = Kernel.CreateBuilder();

builder.AddOpenAIChatCompletion(
    modelId: "gpt-4o-mini",
    apiKey: apiKey);

var kernel = builder.Build();

var chat = kernel.GetRequiredService<IChatCompletionService>();
*/

Console.WriteLine("Hello World!");