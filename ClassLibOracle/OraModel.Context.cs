﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClassLibOracle
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class OraContext : DbContext
    {
        public OraContext()
            : base("name=OraContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
    
        public virtual int RUN_PROC_GUILD_OPC(string iNS_G_UCHASTOK, int iNS_N_STAN, bool iNS_START_STOP, bool iNS_ERASE, bool iNS_BREAK, bool iNS_REPLAC, int iNS_COUNTER, DateTime iNS_INCOMIN_DATE)
        {
            var iNS_G_UCHASTOKParameter = new ObjectParameter("INS_G_UCHASTOK", iNS_G_UCHASTOK);

            var iNS_N_STANParameter = new ObjectParameter("INS_N_STAN", iNS_N_STAN);

            var iNS_START_STOPParameter = new ObjectParameter("INS_START_STOP", iNS_START_STOP);

            var iNS_ERASEParameter = new ObjectParameter("INS_ERASE", iNS_ERASE);

            var iNS_BREAKParameter = new ObjectParameter("INS_BREAK", iNS_BREAK);

            var iNS_REPLACParameter = new ObjectParameter("INS_REPLAC", iNS_REPLAC);

            var iNS_COUNTERParameter = new ObjectParameter("INS_COUNTER", iNS_COUNTER);

            var iNS_INCOMIN_DATEParameter = new ObjectParameter("INS_INCOMIN_DATE", iNS_INCOMIN_DATE);

            var retOut = new ObjectParameter("RET_OUT", typeof(decimal));
            ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("RUN_Proc_GUILD_OPC", iNS_G_UCHASTOKParameter, iNS_N_STANParameter, iNS_START_STOPParameter, iNS_ERASEParameter, iNS_BREAKParameter, iNS_REPLACParameter, iNS_COUNTERParameter, iNS_INCOMIN_DATEParameter, retOut);

            int r = Convert.ToInt32(retOut.Value);

            return r;
        }
    }
}
