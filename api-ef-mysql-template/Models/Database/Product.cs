using System;
using System.Collections.Generic;

namespace api_ef_mysql_template.Models.Database;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }
}
