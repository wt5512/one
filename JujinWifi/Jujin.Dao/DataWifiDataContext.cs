using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jujin.Dao
{
    public partial class DataWifiDataContext
    {
        public DataWifiDataContext() :
            base(System.Configuration.ConfigurationManager.ConnectionStrings["constr"].ConnectionString)
        {
            OnCreated();
        }
    }
}
