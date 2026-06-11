using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FinanceContext>(opt => opt.UseSqlite("Data Source=finance.db"));

var app = builder.Build();

// Ensure database created and seed categories
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
    db.Database.EnsureCreated();
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(new Category { Name = "Food" }, new Category { Name = "Transport" }, new Category { Name = "Rent" }, new Category { Name = "Salary" }, new Category { Name = "Misc" });
        db.SaveChanges();
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/categories", async (FinanceContext db) =>
    await db.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToListAsync());

app.MapGet("/api/transactions", async (FinanceContext db) =>
    await db.Transactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).Take(100).ToListAsync());

app.MapPost("/api/transactions", async (TransactionDto dto, FinanceContext db) =>
{
    var tx = new Transaction
    {
        Date = dto.Date ?? DateTime.UtcNow,
        Amount = dto.Amount,
        Category = dto.Category ?? string.Empty,
        Note = dto.Note ?? string.Empty
    };
    db.Transactions.Add(tx);
    await db.SaveChangesAsync();
    return Results.Created($"/api/transactions/{tx.Id}", tx);
});

app.MapDelete("/api/transactions/{id}", async (int id, FinanceContext db) =>
{
    var tx = await db.Transactions.FindAsync(id);
    if (tx == null) return Results.NotFound();
    db.Transactions.Remove(tx);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/api/summary", async (FinanceContext db) =>
{
    var total = await db.Transactions.SumAsync(t => (decimal?)t.Amount) ?? 0m;
    var income = await db.Transactions.Where(t => t.Amount > 0).SumAsync(t => (decimal?)t.Amount) ?? 0m;
    var expenses = await db.Transactions.Where(t => t.Amount < 0).SumAsync(t => (decimal?)t.Amount) ?? 0m;
    var byCategory = await db.Transactions.GroupBy(t => t.Category)
        .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
        .ToListAsync();

    return Results.Json(new { balance = total, income = income, expenses = -expenses, byCategory });
});

app.MapGet("/api/transactions/export", async (FinanceContext db) =>
{
    var rows = await db.Transactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).ToListAsync();
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("Id,Date,Amount,Category,Note");
    foreach (var r in rows)
    {
        var date = r.Date.ToString("yyyy-MM-dd");
        var note = (r.Note ?? string.Empty).Replace("\"", "\"\"");
        sb.AppendLine($"{r.Id},{date},{r.Amount},{r.Category},\"{note}\"");
    }
    return Results.Text(sb.ToString(), "text/csv");
});

app.MapFallbackToFile("index.html");

app.Run();

// DTO and models
public record TransactionDto(DateTime? Date, decimal Amount, string? Category, string? Note);

public class Transaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class FinanceContext : DbContext
{
    public FinanceContext(DbContextOptions<FinanceContext> options) : base(options) { }
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
}
