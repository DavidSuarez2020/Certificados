using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Certificados.Models;

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
            List<Comerciantes> lst = new List<Comerciantes>();
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                lst = (from p in db.Comerciantes
                       select p).ToList();
            }
            return Json(new { data = lst }, JsonRequestBehavior.AllowGet);
        }
    }
}