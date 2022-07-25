# Logging and Exception Handling Middleware


## What is ASP.NET Core Middleware

ASP.NET Core apps created with the web templates contain the application startup code in the `Program.cs` file. The `Program.cs` file is where:

* Services required by the app are configured.
* The app's **request handling pipeline** is defined as a series of middleware components.

The request handling pipeline is composed as a series of middleware components. Each component performs operations on an [HttpContext](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-6.0) and so each component:

* Chooses whether to pass the request to the next component in the pipeline.
* Can perform work before and after the next component in the pipeline.

The ASP.NET Core request pipeline consists of a **sequence of request delegates**, called one after the other. The following diagram demonstrates the concept. The thread of execution follows the black arrows.

![request-delegate-pipeline](/readme_assets/request-delegate-pipeline.png)


We can also write custom middleware considering our needs and in this lab we will be implementing custom middleware for logging and exception handling.

### Middleware order
The following diagram shows the complete request processing pipeline for ASP.NET Core MVC and Razor Pages apps. You can see how, in a typical app, existing middlewares are ordered and where custom middlewares are added. You have full control over how to reorder existing middlewares or inject new custom middlewares as necessary for your scenarios.

![request-delegate-pipeline](/readme_assets/middleware-pipeline.svg)

The order that middleware components are added in the `Program.cs` file defines the order in which the middleware components are invoked on requests and the reverse order for the response. The order is critical for security, performance, and functionality.

---------------

## About this exercise

In this lab we will be working on **Backend codebase** .

### **Backend Codebase:**

Previously we developed a base structure of an api solution in asp.net core that have just two api functions **GetLast12MonthBalances** & **GetLast12MonthBalances/{userId}** which returns data of the last 12 months total balances.

![apimethods](/readme_assets/apimethods.jpg)


There are 4 Projects in the solution. 

*	**Entities** : This project contains DB models like *User* where each User has one *Account* and each Account can have one or many *Transaction*. There is also a Response Model of *LineGraphData* that will be returned as API Response. 

*	**Infrastructure**: This project contains *BBBankContext* that serves as fake DBContext that populates one User with its corresponding Account that has some Transactions dated of last twelve months with hardcoded data. 

* **Services**: This project contains *TransactionService* with the logic of converting Transactions into LineGraphData after fetching them from BBBankContext.

* **BBBankAPI**: This project contains *TransactionController* with two GET methods *GetLast12MonthBalances* & *GetLast12MonthBalances/{userId}* to call the *TransactionService*.

![apiStructure](/readme_assets/apistructure.png)

For more details about this base project see: [Service Oriented Architecture Lab](https://github.com/PatternsTechGit/PT_ServiceOrientedArchitecture)

---------------
## In this exercise

In this lab we will

* Implement asp.net core default logging in controller 
* Implement logging using custom middleware 
* Implement exception handling using custom middleware

Here are the steps to begin with

## Step 1: Implement Logging in API Method

ASP.NET Core project templates use [Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-6.0#get-started) by default when not hosted with IIS. In `Program.cs`, the **WebApplication.CreateBuilder** method calls [UseKestrel](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hosting.webhostbuilderkestrelextensions.usekestrel?view=aspnetcore-6.0) internally and when we will implement logging then log messages would be shown on kestrel output window.

So now we will implement asp.net core [default logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0) into `TransactionController` . For this we needs to inject logging service into our controller so first define following logging service property in `TransactionController`

```cs
private readonly ILogger<TransactionController> _logger;
```

and inject **logger** in controller constructor for dependency injection

```cs
public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
{
    _transactionService = transactionService;
    _logger = logger;
}
```

Now in controller `GetLast12MonthBalances` function we will implement logging, before and after executing service and in catch block as well so that in case of any exception, error could be logged.

```cs
[HttpGet]
[Route("GetLast12MonthBalances")]
public async Task<ActionResult> GetLast12MonthBalances()
{
    try
    {
        _logger.LogInformation("Executing GetLast12MonthBalances");
        var res = await _transactionService.GetLast12MonthBalances(null);
        _logger.LogInformation("Executed GetLast12MonthBalances");
        return new OkObjectResult(res);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,"Exception executing GetLast12MonthBalances");
        return new BadRequestObjectResult(ex);
    }
}
```

Now execute code and access `GetLast12MonthBalances` and log messages would be shown on **kestrel output window** as:

> ![kestrel-logging-without-middleware](/readme_assets/kestrel-logging-without-middleware.png)

## Step 2: Implement Custom Logging Middleware

Create **Middlewares** folder in API project and add a class `LoggingMiddleware.cs` in it and put following code.
This code is injecting logger in middleware constructor as dependency injection. `InvokeAsync` method is invoked in request handling pipeline and it would be performing operations on **HttpContext**, in our case we will use it for logging the request path .

```cs
  public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        // Middleware implementation must include 
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // this line will be executed when it enters the custom middleware and by that time it has the information about the method its trying to execute using the context.
            _logger.LogInformation("Entering " + context.Request.Path);
            // Call the next delegate/middleware in the pipeline which will be MVC and it will execute function that was called.
            await _next(context);
            // this line will be called after MVC is executed.
            _logger.LogInformation("Leaving " + context.Request.Path);
        }
    }

```


## Step 3: Implement Exception Handling in Middleware

We would implement exception handling in same middleware class `LoggingMiddleware.cs` and would wrap the `InvokeAsync` body in **try** and **catch** block

```cs
  public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        // Middleware implementation must include 
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // this line will be executed when it enters the custom middleware and by that time it has the information about the method its trying to execute using the context.
                _logger.LogInformation("Entering " + context.Request.Path);
                // Call the next delegate/middleware in the pipeline which will be MVC and it will execute function that was called.
                await _next(context);
                // this line will be called after MVC is executed.
                _logger.LogInformation("Leaving " + context.Request.Path);
             }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }

```


## Step 4: Expose And Register Middleware

Now we need to expose middleware as extension method so that it can be injected in asp.net core pipeline in `program.cs`.
Create new **static** class `CustomMiddlewaresExtensions.cs` and use following code in it and class would look like this

```cs
public static class CustomMiddlewaresExtensions
{
    // Extension method exposing the middleware using IApplicationBuilder:
    public static IApplicationBuilder UseCustomLogginMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LoggingMiddleware>();
    }

}
```
And to register middleware in pipeline put this line `app.UseCustomLogginMiddleware();` in `program.cs` before `app.MapControllers();` line. 

Now our logging and exception handling middleware code is completed. 

## Step 5: Remove Logging From Controller

Now we will remove logging from controller as custom middleware logging would be used. Now `TransactionController` constructor  would be look like this.

```cs
public TransactionController(ITransactionService transactionService)
{
    _transactionService = transactionService;
}
```

and in controller `GetLast12MonthBalances` method we will remove logging statements so now method code would be very minimal as we moved logging and exception handling in middleware.

```cs
public async Task<ActionResult> GetLast12MonthBalances()
{
        var res = await _transactionService.GetLast12MonthBalances(null);
        return new OkObjectResult(res);
}

```

------------

## Final Output

Now to see logging in action, run the api project and access API http://localhost:5070/api/Transaction/GetLast12MonthBalances then following logging information should be displayed in kestrel output window

> ![logging-with-middleware](/readme_assets/logging-with-middleware.png)

To see the exception we need to throw exception ourselves in API method , so put this line `throw new Exception("Test middleware exception.");` before return statement:

```cs
public async Task<ActionResult> GetLast12MonthBalances()
{
    var res = await _transactionService.GetLast12MonthBalances(null);
    throw new Exception("Test middleware exception.");
    return new OkObjectResult(res);
}
```

Now run the api project again and access `GetLast12MonthBalances` API method and you should see exception in kestrel output window

> ![exception-handling-with-middleware](/readme_assets/exception-handling-with-middleware.png)