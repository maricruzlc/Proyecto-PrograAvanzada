//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class equipo_salas
    {
        public int Id_equipo_salas { get; set; }
        public Nullable<int> Id_Equipo { get; set; }
        public Nullable<int> IdSala { get; set; }
    
        public virtual equipo equipo { get; set; }
        public virtual sala sala { get; set; }
    }
}
