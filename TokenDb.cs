using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


    public class TokenDb : DbContext
    {
        public DbSet<Token> Tokens { get; set; }
        private static bool IsInitialized = false;
        private static object Mutex = new object();

        public TokenDb(DbContextOptions<TokenDb> options) : base(options){
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