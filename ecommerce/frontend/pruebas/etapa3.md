
dotnet ef migrations add NombreDeTuMigracion --project Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj --startup-project Ecommerce.API/Ecommerce.API.csproj --output-dir Persistence/Migrations

dotnet ef database update --project Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj --startup-project Ecommerce.API/Ecommerce.API.csproj