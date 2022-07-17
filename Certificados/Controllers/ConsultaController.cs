using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Certificados.Models;
using Certificados.Models.ViewModel;

namespace Certificados.Controllers
{
    public class ConsultaController : Controller
    {
        // GET: Consulta
        public ActionResult Index()
        {
            return View();
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