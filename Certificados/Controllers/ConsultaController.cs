using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Certificados.Models;
using Certificados.Models.ViewModel;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Certificados.Reportes;
using System.IO;

namespace Certificados.Controllers
{
    public class ConsultaController : Controller
    {
        // GET: Consulta
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult ReporteInstitucion()
        {
            return View();
        }

        public ActionResult VerReporte()
        {
            var reporte = new ReportClass();
            reporte.FileName = Server.MapPath("/Reportes/ReporteInstitucion.rpt");

            //Conexión para el reporte
            var coninfo = ReportesConexion.GetConnection();
            TableLogOnInfo logOnInfo = new TableLogOnInfo();
            Tables tables;
            tables = reporte.Database.Tables;

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            Stream stream = reporte.ExportToStream(ExportFormatType.PortableDocFormat);
            return new FileStreamResult(stream, "application/pdf");

        }


        public JsonResult Listar()
        {

            List<ComercianteViewModel> lst = new List<ComercianteViewModel>();
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                lst = (from p in db.Comerciantes
                       join a in db.Institucion
                       on p.Institucion equals a.Id
                       select new ComercianteViewModel
                       {
                           Id = p.Id,
                           Nombres = p.Nombres,
                           Apellidos = p.Apellidos,
                           Cedula = p.Cedula,
                           Capacitacion = p.Capacitacion,
                           Institucion = a.Nombre
                       }
                       ).ToList();
            }
            return Json(new { data = lst }, JsonRequestBehavior.AllowGet);
        }
    }
}