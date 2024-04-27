using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class TasksContext: DbContext
    {
        public TasksContext(DbContextOptions<TasksContext> options): base(options) { }

        public DbSet<AllTasks> allTasks { get; set; }
    }
}
