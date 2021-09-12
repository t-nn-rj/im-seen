﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IMWebAPI.Models;

namespace IMWebAPI.Data
{
    public class IM_API_Context : DbContext
    {
        public IM_API_Context (DbContextOptions<IM_API_Context> options)
            : base(options)
        {
        }

        public DbSet<IMWebAPI.Models.Student> Students { get; set; }
        public DbSet<IMWebAPI.Models.User> Users { get; set; }
        public DbSet<IMWebAPI.Models.Observation> Observations { get; set; }
        public DbSet<IMWebAPI.Models.Group> Group { get; set; }
        public DbSet<IMWebAPI.Models.Delegation> Delegation { get; set; }
        public DbSet<IMWebAPI.Models.Supporter> Supporter { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Observation>().ToTable("Observations");
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<Delegation>().ToTable("Delegations");
            modelBuilder.Entity<Supporter>().ToTable("Supporters");

            base.OnModelCreating(modelBuilder);
        }
    }
}
