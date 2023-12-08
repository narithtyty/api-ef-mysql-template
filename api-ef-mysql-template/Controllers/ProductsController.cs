using api_ef_mysql_template;
using EntityFrameworkCore.MySQL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AppDbContextExtensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EntityFrameworkCore.MySQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        public ProductsController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("addproduct")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            _appDbContext.Products.Add(product);
            await _appDbContext.BulkSaveChangesAsync();

            return ResponseResult.Success(200, product);
        }
        [HttpGet]
        [Route("getallproducts")] 
        public async Task<IActionResult> GetAllProducts()
        {
            //string sqlQuery = "SELECT * FROM Products";
            //var product = _appDbContext.SqlQuery(sqlQuery, null);

            //PaginationOptions pagination = new PaginationOptions
            //{
            //    PageNumber = 1,
            //    PageSize = 20
            //};

            //var directSqlQuery = "SELECT * FROM Products";
            //var resultDirectQuery = await _appDbContext.ExecuteQueryOrStoredProcedure(directSqlQuery, null, pagination);

            var queryExtensions = new QueryExtensions<Product>();
            var pageResult = queryExtensions.GetPageResult(_appDbContext.Products.AsQueryable(), 2, 10);

            return ResponseResult.Success(200, pageResult);
        }

        [HttpGet]
        [Route("getproduct/{id}")] 
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _appDbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return ResponseResult.NotFound("product not found");
            }

            return ResponseResult.Success(product);
        }
        [HttpGet]
        [Route("deleteproduct/{id}")]
        public async Task<IActionResult> deteteProductById(int id)
        {
            string sql = "DELETE FROM Products WHERE Id = @Id";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@Id", 2 },
            };

            int affectedRows = _appDbContext.ExecuteSqlCommand(sql, parameters);
            _appDbContext.DisposeContext();
            return ResponseResult.Success(affectedRows);
        }
    }
}
