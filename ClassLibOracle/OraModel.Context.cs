﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
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
            : base("name=OraConnString")
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
            ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("RUN_PROC_GUILD_OPC", iNS_G_UCHASTOKParameter, iNS_N_STANParameter, iNS_START_STOPParameter, iNS_ERASEParameter, iNS_BREAKParameter, iNS_REPLACParameter, iNS_COUNTERParameter, iNS_INCOMIN_DATEParameter, retOut);

            int r = Convert.ToInt32(retOut.Value);

            return r;
        }

        //public virtual int RUN_PROC_GUILD_OPC(string iNS_G_UCHASTOK, Nullable<decimal> iNS_N_STAN, Nullable<decimal> iNS_START_STOP, Nullable<decimal> iNS_ERASE, Nullable<decimal> iNS_BREAK, Nullable<decimal> iNS_REPLAC, Nullable<decimal> iNS_COUNTER, Nullable<System.DateTime> iNS_INCOMIN_DATE, ObjectParameter rET_OUT)
        //{
        //    var iNS_G_UCHASTOKParameter = iNS_G_UCHASTOK != null ?
        //        new ObjectParameter("INS_G_UCHASTOK", iNS_G_UCHASTOK) :
        //        new ObjectParameter("INS_G_UCHASTOK", typeof(string));

        //    var iNS_N_STANParameter = iNS_N_STAN.HasValue ?
        //        new ObjectParameter("INS_N_STAN", iNS_N_STAN) :
        //        new ObjectParameter("INS_N_STAN", typeof(decimal));

        //    var iNS_START_STOPParameter = iNS_START_STOP.HasValue ?
        //        new ObjectParameter("INS_START_STOP", iNS_START_STOP) :
        //        new ObjectParameter("INS_START_STOP", typeof(decimal));

        //    var iNS_ERASEParameter = iNS_ERASE.HasValue ?
        //        new ObjectParameter("INS_ERASE", iNS_ERASE) :
        //        new ObjectParameter("INS_ERASE", typeof(decimal));

        //    var iNS_BREAKParameter = iNS_BREAK.HasValue ?
        //        new ObjectParameter("INS_BREAK", iNS_BREAK) :
        //        new ObjectParameter("INS_BREAK", typeof(decimal));

        //    var iNS_REPLACParameter = iNS_REPLAC.HasValue ?
        //        new ObjectParameter("INS_REPLAC", iNS_REPLAC) :
        //        new ObjectParameter("INS_REPLAC", typeof(decimal));

        //    var iNS_COUNTERParameter = iNS_COUNTER.HasValue ?
        //        new ObjectParameter("INS_COUNTER", iNS_COUNTER) :
        //        new ObjectParameter("INS_COUNTER", typeof(decimal));

        //    var iNS_INCOMIN_DATEParameter = iNS_INCOMIN_DATE.HasValue ?
        //        new ObjectParameter("INS_INCOMIN_DATE", iNS_INCOMIN_DATE) :
        //        new ObjectParameter("INS_INCOMIN_DATE", typeof(System.DateTime));

        //    return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("RUN_PROC_GUILD_OPC", iNS_G_UCHASTOKParameter, iNS_N_STANParameter, iNS_START_STOPParameter, iNS_ERASEParameter, iNS_BREAKParameter, iNS_REPLACParameter, iNS_COUNTERParameter, iNS_INCOMIN_DATEParameter, rET_OUT);
        //}
    }
}
