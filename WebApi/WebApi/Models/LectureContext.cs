using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models
{
    public class LectureContext:DbContext
    {
        public LectureContext(DbContextOptions<LectureContext> options)
            : base(options)
        {
        }

        public DbSet<LectureItem> LectureItems { get; set; }
    }
}
