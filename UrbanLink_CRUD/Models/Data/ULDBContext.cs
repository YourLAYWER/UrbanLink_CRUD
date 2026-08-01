using Microsoft.EntityFrameworkCore;



namespace UrbanLink_CRUD.Models.Data
{
    public class ULDBContext : DbContext
    {
        public ULDBContext(DbContextOptions<ULDBContext> options) : base(options)
        {
        }
    }
}
