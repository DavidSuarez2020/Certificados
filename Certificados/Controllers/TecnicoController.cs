using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Certificados.Models;

namespace Certificados.Controllers
{
    public class TecnicoController : Controller
    {
        // GET: Tecnico
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

        public JsonResult Obtener(int ID)
        {
            Comerciantes oComerciante = new Comerciantes();
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                oComerciante = (from p in db.Comerciantes
                                where p.ID == ID
                                select p).FirstOrDefault();
            }
            return Json(oComerciante, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Guardar(Comerciantes oComerciante)
        {
            bool respuesta = true;
            try
            {

                if (oComerciante.ID == 0)
                {
                    using (ComerciantesEntities db = new ComerciantesEntities())
                    {
                        db.Comerciantes.Add(oComerciante);
                        db.SaveChanges();
                    }
                }
                else
                {
                    using (ComerciantesEntities db = new ComerciantesEntities())
                    {
                        Comerciantes tempComerciante = (from p in db.Comerciantes
                                                        where p.ID == oComerciante.ID
                                                        select p).FirstOrDefault();

                        tempComerciante.Cedula = oComerciante.Cedula;
                        tempComerciante.Nombres = oComerciante.Nombres;
                        tempComerciante.Apellidos = oComerciante.Apellidos;
                        tempComerciante.Institucion = oComerciante.Institucion;
                        tempComerciante.Capacitacion = oComerciante.Capacitacion;

                        db.SaveChanges();
                    }

                }
            }
            catch
            {
                respuesta = false;

            }

            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }



        public JsonResult Eliminar(int ID)
        {
            bool respuesta = true;
            try
            {
                using (ComerciantesEntities db = new ComerciantesEntities())
                {
                    Comerciantes oComerciante = new Comerciantes();
                    oComerciante = (from p in db.Comerciantes.Where(x => x.ID == ID)
                                    select p).FirstOrDefault();

                    db.Comerciantes.Remove(oComerciante);
                    db.SaveChanges();
                }
            }
            catch
            {
                respuesta = false;
            }

            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }


    }
}