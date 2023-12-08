using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.MySQL.Models;

public partial class Efmigrationshistory
{
    public string MigrationId { get; set; }

    public string ProductVersion { get; set; }
}
