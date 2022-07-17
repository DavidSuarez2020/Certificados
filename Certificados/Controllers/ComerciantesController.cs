using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using Certificados.Models;
using Rotativa;
using QRCoder;



namespace Certificados.Controllers
{
    public class ComerciantesController : Controller
    {
        // contante para el número del certificado
        private readonly string baseNumCertificado = "ACDC-DCA-CC-";
        private readonly string baseNumRectificaciones = "SRD-";
        private readonly string salt = "certifyYourself";

        // variable para generar PDF
        //public Comerciantes com = new Comerciantes(); -- YA NO TIENE

        // GET: Comerciantes

        
        public ActionResult Index()
        {
            return View();
        }

        /* --------CAMBIO DE LISTAR -----------------
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
        -------- FIN DE CAMBIO DE LISTAR -----------------
        */

        public JsonResult Listar()
        {
            //List<Comerciantes> lst = new List<Comerciantes>();
            List<TempObject> lst = new List<TempObject>();
            using (ComerciantesEntities db = new ComerciantesEntities())
            {
                lst = (from p in db.Comerciantes
                       join c in db.Institucion
                       on p.Id equals c.Id
                       select new TempObject
                       {
                           Nombres = p.Nombres,
                           Apellidos = p.Apellidos,
                           Cedula = p.Cedula,
                           Capacitacion = p.Capacitacion,
                           Institucion = c.Nombre

                       }).ToList();
            }

            if (lst.Any())
            {
                return Json(new { data = lst }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<Comerciantes> listTemp = new List<Comerciantes>();
                Comerciantes tempNull = new Comerciantes()
                {
                    Id = 0,
                    Nombres = "error",
                    Apellidos = "error",
                    Cedula = "error",
                    Capacitacion = 0,
                    Institucion = 1
                };
                listTemp.Add(tempNull);
                return Json(new { data = listTemp.ToList() }, JsonRequestBehavior.AllowGet);
            }
        }

        //temporal
        public class TempObject
        {
            public string Cedula { get; set; }
            public string Nombres { get; set; }
            public string Apellidos { get; set; }
            public int Capacitacion { get; set; }
            public string Institucion { get; set; }
        }


        public ActionResult NuevaBusqueda()
        {
            return View();
        }


        /* ------- BUSCAR CERTIFICADO ANTERIOR ------------------ 
        [HttpPost]
        public ActionResult BuscarCertificado(string cedulaString)
        {
            // comprobar conversión de texto ingresado
            if (Int32.TryParse(cedulaString, out int cedulaInt))
            {
                ComerciantesEntities db = new ComerciantesEntities();
                var resultado = from c in db.Comerciantes
                                where c.Cedula == cedulaInt.ToString()
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

        ------- FIN BUSCAR CERTIFICADO ANTERIOR ------------------ 
        */



        /* ------- RESTO DE CODIGO ANTERIOR ------------------ 
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
        ------- FIN RESTO DE CODIGO ANTERIOR ------------------ 
        */

        [HttpPost]
        public ActionResult BuscarCertificado(string cedulaString, string apellidosString)  // buscar Comerciante en BDD por 
        {
            // texto ingresada para búsqueda
            ViewBag.DataCedula = cedulaString;
            ViewBag.DataApellidos = apellidosString;
            ViewBag.CertificadoGenerado = false;
            ViewBag.RectificacionGenerado = false;

            // validación de extensión de cedulaString (10 caracteres)
            if (cedulaString.Length == 10)
            {
                // comprobar si tiene certificado generado
                Comerciantes comercianteTemp = GetComercianteByCedulaApellidos(cedulaString, apellidosString);

                if (comercianteTemp != null)
                {
                    if (GetCertificadoByComerId(comercianteTemp.Id) != null)
                    {
                        ViewBag.CertificadoGenerado = true;
                    }
                    else
                    {
                        // comprobar si tiene rectificación generada
                        if (GetRectificacionByComerId(comercianteTemp.Id) != null)
                        {
                            ViewBag.RectificacionGenerado = true;
                        }
                    }
                    return View(comercianteTemp);
                }
                else
                {
                    return View("NoResultado");
                }
            }
            else
            {
                return View("NoResultado");
            }
        }


        public ActionResult GenerarCertificado(Comerciantes comerciante)    // generar Certificado PDF para descargar
        {
            string applicationpath = Request.ApplicationPath;
            ViewBag.Path = applicationpath;

            // verificar si ya generó el documento para recuperarlo, sino se registra
            Models.Certificados certTemp = GetCertificadoByComerId(comerciante.Id);
            int resultRegistroCert;
            if (certTemp != null)
            {
                ViewBag.CertNumber = baseNumCertificado + certTemp.fecha_emision.Year + "-" + certTemp.num_certificado;
            }
            else
            {
                resultRegistroCert = RegistrarCertificado(comerciante);
                if (resultRegistroCert != 0)
                {
                    ViewBag.CertNumber = baseNumCertificado + DateTime.Today.Year + "-" + resultRegistroCert;
                }
                else
                {
                    return View("ProblemaGenerar");
                }
            }

            // generar código QR
            certTemp = new Models.Certificados();
            certTemp = GetCertificadoByComerId(comerciante.Id);
            ViewBag.QRCodeImage = "data:image/png;base64," + GenerarQR(certTemp.codigo_verificacion);

            //return new ViewAsPdf("GenerarCertificado", GetCertificadoByComerId(comerciante.Id))
            return new ViewAsPdf("GenerarCertificado", certTemp)
            {
                FileName = "certificado_" + comerciante.Cedula.ToString(),
                PageSize = Rotativa.Options.Size.A4,
                PageMargins = new Rotativa.Options.Margins(25, 25, 25, 25),
                PageOrientation = Rotativa.Options.Orientation.Portrait
            };
        }


        public int RegistrarCertificado(Comerciantes comerciante)   // registrar Certificado en la BDD
        {
            // crear objeto temporal para su almacenamiento
            Models.Certificados certificadoTemp = new Models.Certificados
            {
                fecha_emision = DateTime.Today,
                comerciantes_id = comerciante.Id,
                codigo_verificacion = CreateHashString(comerciante.Cedula)
            };

            // búsqueda número certificado actual
            ComerciantesEntities dbCert = new ComerciantesEntities();
            var resultNumCert = from cert in dbCert.Certificados
                                select cert.num_certificado;
            if (resultNumCert.Any())
            {
                var numRect = resultNumCert.Max();
                certificadoTemp.num_certificado = ++numRect;
            }
            else
            {
                certificadoTemp.num_certificado = 1;
            }

            // guardar el registro y revisar si fue satisfactorio
            dbCert.Certificados.Add(certificadoTemp);
            if (dbCert.SaveChanges() > 0)
            {
                return certificadoTemp.num_certificado;
            }
            else
            {
                return 0;
            }
        }


        public string GenerarQR(string hashText)   // generar código QR con información del Comerciante
        {
            // información del comerciantes que irá en el código QR
            string dataForQR = $"https://localhost:44320/Comerciantes/ValidarCertificado/{hashText}";

            // generar código QR - información
            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(dataForQR, QRCodeGenerator.ECCLevel.Q);

            // renderizar - representación
            PngByteQRCode qrCode = new PngByteQRCode(qRCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeImage);
        }


        public ActionResult RectificarDatos(Comerciantes comerciante)   // seleccionar datos a rectificar
        {
            ViewBag.Rectificado = false;
            ViewData["comerciante"] = GetComercianteById(comerciante.Id);
            ViewData["institucionesDMQ"] = GetItemsInstitucionesDMQ(GetInstituciones(), comerciante.Id);

            Rectificaciones rectificacion = new Rectificaciones()
            {
                comerciantes_id = comerciante.Id,
                rectificar_inst_origen = comerciante.Id
            };

            return View(rectificacion);
        }

        public ActionResult ValidarCertificado(string id)  // validar certificado generado
        {
            //Comerciantes comerciante = GetComercianteByCedula(id);
            Comerciantes comerciante = GetComercianteByCodigoVerificacion(id);
            ViewBag.CertNumber = baseNumCertificado;
            if (comerciante != null)
            {
                return View(GetCertificadoByComerId(comerciante.Id));
            }
            else
            {
                return View("NoValidacion");
            }
        }


        [HttpPost]
        public ActionResult GenerarRectificacion(Rectificaciones rectificacion) // descargar PDF rectificar ya generado
        {
            // generar número de solicitud
            ComerciantesEntities dbRect = new ComerciantesEntities();
            var resultNumRect = from cert in dbRect.Rectificaciones
                                select cert.num_solicitud;
            if (resultNumRect.Any())
            {
                var numRect = resultNumRect.Max();
                rectificacion.num_solicitud = ++numRect;
            }
            else
            {
                rectificacion.num_solicitud = 1;
            }
            ViewBag.RectNumber = baseNumRectificaciones + DateTime.Today.Year + "-" + rectificacion.num_solicitud;

            // agregar fecha
            rectificacion.fecha_rectificar = DateTime.Now;

            // verificar si rectifica institución (cambio)
            ViewBag.RectificarInstitucion = false;
            ViewData["origen"] = "";
            ViewData["destino"] = "";
            if (rectificacion.rectificar_inst_destino == 0 || rectificacion.rectificar_inst_destino == rectificacion.rectificar_inst_origen)
            {
                rectificacion.rectificar_inst_origen = 0;
            }
            else
            {
                ViewBag.RectificarInstitucion = true;
                ViewData["origen"] = GetInstitucionById(rectificacion.rectificar_inst_origen).Nombre;
                ViewData["destino"] = GetInstitucionById(rectificacion.rectificar_inst_destino).Nombre;
            }

            // guardar en el registro de Rectificaciones
            ComerciantesEntities dbRectGuardar = new ComerciantesEntities();
            dbRectGuardar.Rectificaciones.Add(rectificacion);
            if (dbRectGuardar.SaveChanges() > 0)
            {
                Rectificaciones rectificacionTemp = GetRectificacionByComerId(rectificacion.comerciantes_id);

                return new ViewAsPdf("GenerarRectificacion", rectificacionTemp)
                {
                    FileName = "rectificacion_" + rectificacionTemp.Comerciantes.Cedula,
                    PageSize = Rotativa.Options.Size.A4,
                    PageMargins = new Rotativa.Options.Margins(25, 25, 25, 25),
                    PageOrientation = Rotativa.Options.Orientation.Portrait
                };
            }
            else
            {
                return View("ProblemaGenerar");
            }
        }


        public ActionResult DescargarRectificacion(Comerciantes comerciante)    // generar Rectificacion PDF para descarga
        {
            ViewBag.RectNumber = baseNumRectificaciones + DateTime.Today.Year + "-";
            Rectificaciones rectificacion = GetRectificacionByComerId(comerciante.Id);
            if (rectificacion != null)
            {
                // verificar si rectifica institución
                ViewBag.RectificarInstitucion = false;
                ViewData["origen"] = "";
                ViewData["destino"] = "";
                if (rectificacion.rectificar_inst_destino != rectificacion.rectificar_inst_origen)
                {
                    ViewBag.RectificarInstitucion = true;
                    ViewData["origen"] = GetInstitucionById(rectificacion.rectificar_inst_origen).Nombre;
                    ViewData["destino"] = GetInstitucionById(rectificacion.rectificar_inst_destino).Nombre;
                }

                ViewBag.RectNumber = ViewBag.RectNumber + rectificacion.num_solicitud;
                return new ViewAsPdf("GenerarRectificacion", rectificacion)
                {
                    FileName = "rectificacion_" + comerciante.Cedula,
                    PageSize = Rotativa.Options.Size.A4,
                    PageMargins = new Rotativa.Options.Margins(25, 25, 25, 25),
                    PageOrientation = Rotativa.Options.Orientation.Portrait
                };
            }
            else
            {
                return View("ProblemaGenerar");
            }
        }


        public Comerciantes GetComercianteById(int comerId)      // recuperar Comerciante por su Id
        {
            ComerciantesEntities dbCom = new ComerciantesEntities();
            var resultado = from com in dbCom.Comerciantes
                            where com.Id == comerId
                            select com;
            if (resultado.Any())
            {
                return resultado.First();
            }
            else
            {
                return null;
            }
        }

        public Comerciantes GetComercianteByCedula(string cedulaString)    // buscar Comerciante por cedulaString
        {
            ComerciantesEntities dbCom = new ComerciantesEntities();
            var resultado = from c in dbCom.Comerciantes
                            where c.Cedula == cedulaString
                            select c;
            if (resultado.Any())
            {
                return resultado.First();
            }
            else
            {
                return null;
            }
        }


        public Comerciantes GetComercianteByCedulaApellidos(string cedulaString, string apellidosString)    // buscar Comerciante
        {
            ComerciantesEntities dbCom = new ComerciantesEntities();
            var resultado = from c in dbCom.Comerciantes
                            where c.Cedula == cedulaString && c.Apellidos == apellidosString
                            select c;
            if (resultado.Any())
            {
                return resultado.First();
            }
            else
            {
                return null;
            }
        }

        public Comerciantes GetComercianteByCodigoVerificacion(string codigoVerificacion)    // buscar Comerciante por cedulaString
        {
            ComerciantesEntities dbCom = new ComerciantesEntities();
            var resultado = from com in dbCom.Comerciantes
                            join cert in dbCom.Certificados on com.Id equals cert.comerciantes_id
                            where cert.codigo_verificacion == codigoVerificacion
                            select com;
            if (resultado.Any())
            {
                return resultado.First();
            }
            else
            {
                return null;
            }
        }


        public Models.Certificados GetCertificadoByComerId(int comerId) // recuperar Certificado por id comerciante
        {
            ComerciantesEntities dbCert = new ComerciantesEntities();
            var resultado = from cert in dbCert.Certificados
                            where cert.comerciantes_id == comerId
                            select cert;
            if (resultado.Any())
            {
                return resultado.First();
            }
            else
            {
                return null;
            }
        }

        public Rectificaciones GetRectificacionByComerId(int comerId)   // recuperar Rectificion por id comerciante
        {
            ComerciantesEntities dbRect = new ComerciantesEntities();
            var resultado = from rect in dbRect.Rectificaciones
                            where rect.comerciantes_id == comerId
                            select rect;
            if (resultado.Any())
            {
                return resultado.First();
            }
            else
            {
                return null;
            }
        }


        public Institucion GetInstitucionById(int id)    // recuperar Institucion por su Id
        {
            ComerciantesEntities dbInst = new ComerciantesEntities();
            var resultado = from inst in dbInst.Institucion
                            where inst.Id == id
                            select inst;
            if (resultado.Any())
            {
                return resultado.First();
            }
            else
            {
                return null;
            }
        }

        public List<Institucion> GetInstituciones()    // recuperar listados de instituciones
        {
            ComerciantesEntities dbInst = new ComerciantesEntities();
            var resultado = from inst in dbInst.Institucion
                            select inst;
            if (resultado.Any())
            {
                return resultado.ToList();
            }
            else
            {
                return null;
            }
        }


        public List<SelectListItem> GetItemsInstitucionesDMQ(List<Institucion> institucionesDMQ, int institucion_id)   // recupera InstitucionesDMQ para drop-down
        {
            var listItemsInstituciones = new List<SelectListItem>();
            foreach (var inst in institucionesDMQ)
            {
                bool selectedItem = institucion_id == inst.Id ? true : false;
                listItemsInstituciones.Add(new SelectListItem { Selected = selectedItem, Text = inst.Nombre, Value = inst.Id.ToString() });
            }
            return listItemsInstituciones;
        }

        public string CreateHashString(string text)  // convierte String a SHA256 - String
        {
            if (!String.IsNullOrEmpty(text))
            {
                // inicializar el objeto SHA
                using (var sha = new SHA256Managed())
                {

                    // convierte string a un array de bytes y calcular el hash
                    byte[] textBytes = Encoding.UTF8.GetBytes(text + salt);
                    byte[] hashBytes = sha.ComputeHash(textBytes);

                    // convierte el array de bytes en String y remueve el "-" de BitConverter
                    string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                    return hash;
                }
            }
            else
            {
                return String.Empty;
            }
        }





    } //Fin Controlador
} // Fin de NameSpace