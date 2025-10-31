using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Services.Agents;

public class BehaviorContext
{
    public ApplicationDbContext DbContext { get; set; } = null!;
    public ILogger Logger { get; set; } = null!;
    public Game Game { get; set; } = null!;
    public Dictionary<string, object> SharedData { get; set; } = new();
    public CancellationToken CancellationToken { get; set; }
}
