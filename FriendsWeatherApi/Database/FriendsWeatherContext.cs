using FriendsWeatherApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendsWeatherApi.Database;

public class FriendsWeatherContext : DbContext
{
    public FriendsWeatherContext() {}
    public FriendsWeatherContext(DbContextOptions<FriendsWeatherContext> options) : base(options) {}

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserMask> Masks { get; set; } = null!;
    public DbSet<Friendship> Friendships { get; set; } = null!;
    public DbSet<FriendshipRequest> FriendshipRequests { get; set; } = null!;
    public DbSet<VerifyToken> VerifyTokens { get; set; } = null!;
}