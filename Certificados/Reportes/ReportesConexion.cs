using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Certificados.Reportes
{
    public class ReportesConexion
    {
        public static CrystalDecisions.Shared.ConnectionInfo GetConnection()
        {
            CrystalDecisions.Shared.ConnectionInfo infocon = new CrystalDecisions.Shared.ConnectionInfo();
            infocon.ServerName = @"(localdb)\MSSQLLocalDB";
            infocon.DatabaseName = "Comerciantes";
            infocon.DatabaseName = "Institucion";
            infocon.IntegratedSecurity = true;

            return infocon;
        }
    }
}