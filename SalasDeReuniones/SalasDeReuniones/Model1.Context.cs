﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SalasDeReuniones
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ProyectoPAEntities : DbContext
    {
        public ProyectoPAEntities()
            : base("name=ProyectoPAEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<estadoSala> estadoSalas { get; set; }
        public virtual DbSet<reserva> reservas { get; set; }
        public virtual DbSet<role> roles { get; set; }
        public virtual DbSet<sala> salas { get; set; }
        public virtual DbSet<usuario> usuarios { get; set; }
        public virtual DbSet<equipo> equipoes { get; set; }
        public virtual DbSet<equipo_salas> equipo_salas { get; set; }
    }
}
