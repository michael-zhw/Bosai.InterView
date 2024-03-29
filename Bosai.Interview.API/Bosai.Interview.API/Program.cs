﻿

using Bosai.Interview.Service.Contracts.Leaderboard;
using Bosai.Interview.Service.Leaderboard;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ILeaderboardService, LeaderboardService>();

//Injection queue service
builder.Services.AddSingleton<ScoreUpdateQueue>();
builder.Services.AddSingleton<ScoreUpdateProcessor>();

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

