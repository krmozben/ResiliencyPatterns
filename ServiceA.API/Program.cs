using Polly;
using Polly.Extensions.Http;
using ServiceA.API;
using System.Diagnostics;


IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions.HandleTransientHttpError().OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound).WaitAndRetryAsync(3, retryAttempt =>
    {
        Debug.WriteLine($"Retry Count :{ retryAttempt }");

        return TimeSpan.FromSeconds(10);
    }, onRetryAsync: OnRetryAsync);
}

Task OnRetryAsync(DelegateResult<HttpResponseMessage> arg1, TimeSpan arg2)
{
    Debug.WriteLine($"Request is mage again :{arg2.TotalMilliseconds }");

    return Task.CompletedTask;
}


IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(3, TimeSpan.FromSeconds(10),
        onBreak: (arg1, arg2) =>
        {
            Debug.WriteLine("Circuit Breaker Status => onBreak");
        },
        onReset: () => { 
            Debug.WriteLine("Circuit Breaker Status => onReset"); },
        onHalfOpen: () => { 
            Debug.WriteLine("Circuit Breaker Status => onHalfOpen"); 
        });
}


IAsyncPolicy<HttpResponseMessage> GetAdvanceCircuitBreakerPolicy()
{
    return HttpPolicyExtensions.HandleTransientHttpError().AdvancedCircuitBreakerAsync(0.1, TimeSpan.FromSeconds(30), 3,TimeSpan.FromSeconds(30),
        onBreak: (arg1, arg2) =>
        {
            Debug.WriteLine("Circuit Breaker Status => onBreak");
        },
        onReset: () =>
        {
            Debug.WriteLine("Circuit Breaker Status => onReset");
        },
        onHalfOpen: () =>
        {
            Debug.WriteLine("Circuit Breaker Status => onHalfOpen");
        });
}



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHttpClient<ProductService>(x =>
{
    x.BaseAddress = new Uri("https://localhost:7035/api/products/");
})
    .AddPolicyHandler(GetAdvanceCircuitBreakerPolicy());
//.AddPolicyHandler(GetCircuitBreakerPolicy());
//.AddPolicyHandler(GetRetryPolicy());

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


