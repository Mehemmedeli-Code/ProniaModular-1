using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ProniaModular.Modules.Products.Data
{
    // Lets "dotnet ef" build the context without booting the whole API host, e.g.:
    // dotnet ef migrations add Init --project src/Modules/ProniaModular.Modules.Products --output-dir Data/Migrations
    public sealed class ProductsDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
    {
        private const string HostProjectName = "ProniaModular.API";

        private const string FallbackConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=ProniaModularDb;Trusted_Connection=True;TrustServerCertificate=True;";

        public ProductsDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(ResolveHostProjectPath())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("ProductsDb") ?? FallbackConnectionString;

            var options = new DbContextOptionsBuilder<ProductsDbContext>()
                .UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsAssembly(typeof(ProductsDbContext).Assembly.GetName().Name);
                    sql.MigrationsHistoryTable(ProductsDbContext.MigrationsHistoryTable, ProductsDbContext.Schema);
                })
                .Options;

            return new ProductsDbContext(options);
        }

        // Walks up from the current directory until it finds the API project folder,
        // so the same connection string is used at design time and at run time.
        private static string ResolveHostProjectPath()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, HostProjectName);

                if (File.Exists(Path.Combine(candidate, "appsettings.json")))
                    return candidate;

                if (File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
                    return directory.FullName;

                directory = directory.Parent;
            }

            return Directory.GetCurrentDirectory();
        }
    }
}
