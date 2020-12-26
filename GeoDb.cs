
using Data;
using Microsoft.EntityFrameworkCore;

public class GeoDb : DbContext {

    public DbSet<Item> Items { get; set; }

    private static bool IsInitialized = false;

    private static object Mutex = new object();

    public GeoDb(DbContextOptions<GeoDb> options) : base(options){
        if (IsInitialized)
            return;

        lock (Mutex) {

            if (IsInitialized)
                return;

            // Migrate database
            Database.Migrate();
            IsInitialized = true;
        }
    }
}