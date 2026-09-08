using ProniaModular.Modules.Products;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Required by app.UseExceptionHandler() so the module handler can return ProblemDetails.
builder.Services.AddProblemDetails();


// DbContext + services + validators + exception handler of the Products module.
builder.Services.AddProductsModule(builder.Configuration);


var app = builder.Build();


app.UseExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Creates the database and applies the module migrations on startup.
    await app.MigrateProductsModuleAsync();
}

app.UseHttpsRedirection();


// /api/categories, /api/products, /api/sizes, /api/products/{id}/sizes
app.MapProductsModule();


app.Run();
