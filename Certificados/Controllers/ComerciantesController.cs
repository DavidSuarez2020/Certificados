using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Certificados.Models;
using Rotativa;


namespace Certificados.Controllers
{
    public class ComerciantesController : Controller
    {
        // variable para generar PDF
        public Comerciantes com = new Comerciantes();

        // GET: Comerciantes
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

        public ActionResult NuevaBusqueda()
        {
            return View();
        }

        
        
        [HttpPost]
        public ActionResult BuscarCertificado(string cedulaString)
        {
            // comprobar conversión de texto ingresado
            if (Int32.TryParse(cedulaString, out int cedulaInt))
            {
                ComerciantesEntities db = new ComerciantesEntities();
                var resultado = from c in db.Comerciantes
                                where c.Cedula == cedulaInt
                                select c;

                Comerciantes comercianteResult = new Comerciantes();

                // comprobar si resultado tiene al menos un elemento
                if (resultado.Any())
                {
                    comercianteResult = resultado.First();
                    ViewBag.Data = cedulaString;
                    com = comercianteResult;
                    return View(comercianteResult);
                }
                else
                {
                    ViewBag.Data = cedulaString;
                    ResetCom();
                    return View("NoResultado");
                }
            }
            else
            {
                // en caso que no pueda convertirse el valor ingresado
                ViewBag.Data = cedulaString;
                ResetCom();
                return View("NoResultado");
            }
        }

        public void ResetCom()
        {
            //com = null;
            com = new Comerciantes();
        }

        public ActionResult GenerarPDF(Comerciantes comerciante)
        {
            //return View();
            return new ViewAsPdf("GenerarPDF", comerciante);
        }
    }
}