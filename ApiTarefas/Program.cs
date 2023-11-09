using Microsoft.EntityFrameworkCore;

// Cria um construtor de aplicativo web usando a classe WebApplication, 
// passando os argumentos fornecidos à aplicação.
var builder = WebApplication.CreateBuilder(args);

// Configuração do Swagger para documentação da API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do DbContext usando o Entity Framework Core para um banco de dados em memória.
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

// Configuração do pipeline de solicitação HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapeamento de endpoints para operações CRUD em 'Tarefas'.

// Obtém todas as tarefas.
app.MapGet("/tarefas", async (AppDbContext db) => await db.Tarefas.ToListAsync());

// Obtém uma tarefa por ID.
app.MapGet("/tarefas/{id}", async (int id, AppDbContext db) =>
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

// Obtém todas as tarefas concluídas.
app.MapGet("/tarefas/concluidas", async (AppDbContext db) =>
    await db.Tarefas.Where(t => t.IsConcluida).ToListAsync());

// Adiciona uma nova tarefa.
app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});

// Atualiza uma tarefa existente por ID.
app.MapPut("/tarefas/{id}", async (int id, Tarefa inputTarefa, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    if (tarefa is null) return Results.NotFound();

    tarefa.Nome = inputTarefa.Nome;
    tarefa.IsConcluida = inputTarefa.IsConcluida;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Remove uma tarefa por ID.
app.MapDelete("/tarefas/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }
    return Results.NotFound();
});

// Executa o aplicativo.
app.Run();

// Definição da classe Tarefa.
class Tarefa
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public bool IsConcluida { get; set; }
}

// Definição da classe AppDbContext que herda de DbContext.
class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}
