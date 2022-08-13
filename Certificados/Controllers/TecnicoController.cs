using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Certificados.Models;
using Certificados.Models.ViewModel;

namespace Certificados.Controllers
{
    public class TecnicoController : Controller
    {
        // GET: Tecnico
        public ActionResult Index()
        {
            List<InstitucionViewModel> inst = null;
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                inst = (from d in db.Institucion
                        select new InstitucionViewModel
                        {
                            ID = d.Id,
                            Nombre = d.Nombre
                        }).ToList();
            }

            List<SelectListItem> items = inst.ConvertAll(d =>
            {

                return new SelectListItem()
                {
                    Text = d.Nombre.ToString(),
                    Value = d.ID.ToString(),
                };
                
            });

            ViewBag.items = items;
            return View();
        }

        public ActionResult SolicitudesRectificacion()
        {
            List<InstitucionViewModel> inst = null;
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                inst = (from d in db.Institucion
                        select new InstitucionViewModel
                        {
                            ID = d.Id,
                            Nombre = d.Nombre
                        }).ToList();
            }
            List<SelectListItem> items = inst.ConvertAll(d =>
            {

                return new SelectListItem()
                {
                    Text = d.Nombre.ToString(),
                    Value = d.ID.ToString(),
                };

            });
            ViewBag.items = items;
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
                           InstitucionID = a.Id,
                           Institucion = a.Nombre
                       }
                       ).ToList();
            }
            return Json(new { data = lst }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListarComerciantes()
        {
            List<ComercianteCertificadoViewModel> listaResultado = new List<ComercianteCertificadoViewModel>();
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                listaResultado = (from comer in db.Comerciantes
                                  select new ComercianteCertificadoViewModel
                                  {
                                      Id = comer.Id,
                                      Nombres = comer.Nombres,
                                      Apellidos = comer.Apellidos,
                                      Cedula = comer.Cedula,
                                      Institucion = comer.Institucion1.Nombre,
                                      CertificadoGenerado = "No"
                                  }
                       ).ToList();

                foreach (ComercianteCertificadoViewModel cc in listaResultado)
                {
                    if (BuscarCertificadoByComercianteId(cc.Id))
                    {
                        cc.CertificadoGenerado = "Sí";
                    }
                }
            }
            return Json(new { data = listaResultado }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListarSolicitudesRectificacion()
        {
            List<ComercianteRectificacionViewModel> lst = new List<ComercianteRectificacionViewModel>();
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                lst = (from p in db.Comerciantes
                       join rect in db.Rectificaciones on p.Id equals rect.comerciantes_id
                       where rect.solicitud_atendida == false
                       select new ComercianteRectificacionViewModel
                       {
                           Id = p.Id,
                           Nombres = p.Nombres,
                           Apellidos = p.Apellidos,
                           Cedula = p.Cedula,
                           Institucion = p.Institucion1.Nombre,
                           RectificacionGenerada = "Sí"
                       }).ToList();
                //RectificacionGenerada = rect.Id != 0 ? "Sí" : "No"
            }
            return Json(new { data = lst }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Obtener(int ID)
        {
            ComercianteViewModel oComerciante = new ComercianteViewModel();
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                oComerciante = (from p in db.Comerciantes
                       join a in db.Institucion
                       on p.Id equals ID
                       select new ComercianteViewModel
                       {
                           Id = p.Id,
                           Nombres = p.Nombres,
                           Apellidos = p.Apellidos,
                           Cedula = p.Cedula,
                           Capacitacion = p.Capacitacion,
                           InstitucionID = a.Id,
                           Institucion = a.Nombre
                       }).ToList().FirstOrDefault();
            }
            return Json(oComerciante, JsonRequestBehavior.AllowGet);
        }

        


        [HttpPost]
        public JsonResult Guardar(Comerciantes oComerciante)
        {
            //(VERIFICAR DONDE VA)  ViewData["institucion"] = GetItemsInstitucion(GetInstituciones(), Comerciantes.institucion_ID);
            //DROPDOWN ANTERIOR @Html.DropDownListFor("Institucion", items, "Seleccione la Institución", new { @class = "form-control" })
            bool respuesta = true;
            try
            {

                if (oComerciante.Id == 0)
                {
                    using (ComerciantesEntities db = new ComerciantesEntities())
                    {
                        db.Comerciantes.Add(oComerciante);
                        db.SaveChanges();
                    }
                }
                else
                {
                    
                    //ComercianteViewModel tempComerciante = new ComercianteViewModel();
                    using (ComerciantesEntities db = new ComerciantesEntities())
                    {
                        Comerciantes tempComerciante = (from p in db.Comerciantes
                                                        join a in db.Institucion
                                                        on p.Id equals oComerciante.Id
                                                        
                                                        select p).FirstOrDefault();

                        tempComerciante.Nombres = oComerciante.Nombres;
                        tempComerciante.Apellidos = oComerciante.Apellidos;
                        tempComerciante.Capacitacion = oComerciante.Capacitacion;
                        tempComerciante.Cedula = oComerciante.Cedula;

                        

                        /*
                        tempComerciante.Institucion = oComerciante.Institucion;
                        */
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


        [HttpPost]
        public JsonResult GuardarAtencion(Comerciantes oComerciante)
        {
            bool respuesta = true;
            try
            {
                if (oComerciante.Id == 0)
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
                        // actualizar datos comerciante
                        Comerciantes tempComerciante = (from p in db.Comerciantes
                                                        join a in db.Institucion
                                                        on p.Id equals oComerciante.Id

                                                        select p).FirstOrDefault();

                        tempComerciante.Nombres = oComerciante.Nombres;
                        tempComerciante.Apellidos = oComerciante.Apellidos;
                        tempComerciante.Capacitacion = oComerciante.Capacitacion;
                        tempComerciante.Cedula = oComerciante.Cedula;

                        // actualizar atención de rectificación
                        Rectificaciones tempRectificacion = (from r in db.Rectificaciones
                                                             where r.comerciantes_id == oComerciante.Id
                                                             select r).FirstOrDefault();

                        tempRectificacion.solicitud_atendida = true;

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
                    oComerciante = (from p in db.Comerciantes.Where(x => x.Id == ID)
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


        public bool BuscarCertificadoByComercianteId(int comerId)
        {
            bool resultado = false;
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                resultado = (from cert in db.Certificados
                             where cert.comerciantes_id == comerId
                             select cert).Any();
            }
            return resultado;
        }

        public bool BuscarRectificacionByComercianteId(int comerId)
        {
            bool resultado = false;
            try
            {
                using (ComerciantesEntities db = new ComerciantesEntities())
                {
                    resultado = (from rect in db.Rectificaciones
                                 where rect.comerciantes_id == comerId
                                 select rect).Any();
                }
                return resultado;
            }
            catch (Exception)
            {
                return resultado;
            }
        }

        public List<Institucion> GetInstituciones()
        {
            ComerciantesEntities dbInst = new ComerciantesEntities();
            var resultado = from inst in dbInst.Institucion select inst;
            if(resultado.Any())
            {
                return resultado.ToList();
            }
            else
            {
                return null;
            }
        }

        public List<SelectListItem>GetItemsInstitucionesDMQ(List<Institucion>institucionesDMQ, int institucion_id)
        {
            var items = new List<SelectListItem>();
            foreach(var inst in institucionesDMQ)
            {
                bool selectedItem = institucion_id == inst.Id ? true : false;
                items.Add(new SelectListItem { Selected = selectedItem, Text = inst.Nombre, Value = inst.Id.ToString() });
            }
            return ViewBag(items);
        }

        //FIN Codigo


        


    }
}