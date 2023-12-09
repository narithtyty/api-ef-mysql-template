using api_ef_mysql_template.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace EntityFrameworkCore.MySQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DBContext _DbContext;
        public ProductsController(DBContext DbContext)
        {
            _DbContext = DbContext;
        }

        [HttpPost]
        [Route("addproduct")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            try
            {
                _DbContext.Products.Add(product);
                await _DbContext.BulkSaveChangesAsync();

                return ResponseResult.Success(200, product);
            }
            catch(Exception ex)
            {
                return ResponseResult.Error(ex.Message);
            }
        }
        [HttpGet]
        [Route("getallproducts")]
        //[ApiActionFilter(Scope = ScopeEnum.API, Role = "user")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                //var key = "rith_so_cool";
                //var tokenService = new TokenService(key);

                //// Generate a token
                //var userId = "123";
                //var username = "rith";
                //var token = tokenService.GenerateToken(userId, username);

                //// Validate a token
                //var validatedPrincipal = tokenService.ValidateToken(token);



                //string sqlQuery = "SELECT * FROM Products";
                //var product = _appDbContext.SqlQuery(sqlQuery, null);

                //PaginationOptions pagination = new PaginationOptions
                //{
                //    PageNumber = 1,
                //    PageSize = 20
                //};

                //var directSqlQuery = "SELECT * FROM Products";
                //var resultDirectQuery = await _appDbContext.ExecuteQueryOrStoredProcedure(directSqlQuery, null, pagination);

                string sql = "SELECT * FROM Products";
                var result = _DbContext.SqlQuery(sql);
                
                string filePath = "output.xlsx";
                ExcelExtensions.CreateExcelFile(result, filePath);

                var queryExtensions = new QueryExtensions<Product>();
                var pageResult = queryExtensions.GetPageResult(_DbContext.Products.AsQueryable(), 1, 10);

                return ResponseResult.Success(200, pageResult);
            }catch(Exception ex)
            {
                return ResponseResult.Error(ex.Message);
            }
            
        }

        [HttpGet]
        [Route("getproduct/{id}")] 
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _DbContext.Products
               .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return ResponseResult.NotFound("product not found");
                }

                return ResponseResult.Success(product);
            }
            catch (Exception ex)
            {
                return ResponseResult.Error(ex.Message);
            }
           
        }
        [HttpGet]
        [Route("deleteproduct/{id}")]
        public async Task<IActionResult> deteteProductById(int id)
        {
            try
            {
                string sql = "DELETE FROM Products WHERE Id = @Id";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@Id", 2 },
                };

                int affectedRows = _DbContext.ExecuteSqlCommand(sql, parameters);
                _DbContext.DisposeContext();
                return ResponseResult.Success(affectedRows);
            }
            catch (Exception ex)
            {
                return ResponseResult.Error(ex.Message);
            }
        }
    }
}
