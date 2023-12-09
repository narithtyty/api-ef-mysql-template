
using System.Threading.Tasks;
using api_ef_mysql_template.Models.Database;
using Quartz;

public class JobExample : IJob
{
    private readonly AppDbContext _appDbContext;
    public JobExample(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public Task Execute(IJobExecutionContext context)
    {
        Product product = new Product()
        {
            Id = 1,
            Name = "Test",
            Type = "test"
        };
        _appDbContext.Products.Add(product);
        _appDbContext.BulkSaveChanges();
        return Task.CompletedTask;
    }
}
