using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Certificados.Models;
using System.IO;
using System.IO.Compression;

namespace Certificados.Controllers
{
    public class DocumentosController : Controller
    {
        private readonly ComerciantesEntities dbContextPlantilla = new ComerciantesEntities();

        public ComerciantesEntities DbContextPlantilla => dbContextPlantilla;

        // GET: Documentos
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListarDocumentos()
        {
            return View(GetDocumentos());
        }

        public ActionResult VerDocumento(string rutaArchivo)
        {
            try
            {
                string pathDescarga = Server.MapPath("~/template_certificados/");
                byte[] fileBytes = System.IO.File.ReadAllBytes(pathDescarga + rutaArchivo);
                return File(fileBytes, "application/pdf", rutaArchivo);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                ViewBag.Mensaje = "No se ha encontrado el archivo indicado.";
                ViewBag.MensajeError = "Error: " + fileNotFoundException.Message.ToString();
                return View("ReporteError");
            }
            catch (Exception e)
            {
                ViewBag.Mensaje = "Se ha presentado un problema al intentar ver la plantilla.";
                ViewBag.MensajeError = "Error: " + e.Message.ToString();
                return View("ReporteError");
            }
        }

        public ActionResult DetallarDocumento(int idDoc)
        {
            if (ModelState.IsValid)
            {
                Documentos docDetalles = GetDocumentoById(idDoc);
                if (docDetalles != null)
                {
                    return View(docDetalles);
                }
                else
                {
                    ViewBag.Mensaje = "No se encuentran detalles de este documento.";
                    ViewBag.MensajeError = "Por favor, intente más tarde";
                    return View("ReporteError");
                }
            }
            else
            {
                ViewBag.Mensaje = "No se ha podido atender su requerimiento debido a un error del sistema.";
                ViewBag.MensajeError = "Por favor, intente más tarde";
                return View("ReporteError");
            }
        }

        public JsonResult Descargar(string[] docsIdArray)   // prueba ResultJson a AJAX para descarga
        {
            string listaString = String.Empty;
            foreach (var docId in docsIdArray)
            {
                listaString = listaString + docId + ",";
            }
            return Json(data: new { lista = listaString });
        }

        public ActionResult DescargarZipDocumentos(string[] docsIdArray)    // descarga ZIP de documentos generados
        {
            try
            {
                if (docsIdArray != null || docsIdArray.Length > 0)
                {
                    // obtener lista de documentos generados
                    List<Documentos> listaDocumentos = new List<Documentos>();
                    foreach (var item in docsIdArray)
                    {
                        if (Int32.TryParse(item, out int datoId))
                        {
                            listaDocumentos.Add(GetDocumentoById(datoId));
                        }
                    }

                    // creación del archivo ZIP
                    string pathDescarga = Server.MapPath("~/template_certificados/");
                    using (var ms = new MemoryStream())
                    {
                        using (var archivo = new ZipArchive(ms, ZipArchiveMode.Create, true))
                        {
                            foreach (Documentos doc in listaDocumentos)
                            {
                                var entry = archivo.CreateEntry(doc.ruta_archivo);
                                using (var entryStream = entry.Open())
                                using (var fileStream = System.IO.File.OpenRead(pathDescarga + doc.ruta_archivo))
                                {
                                    fileStream.CopyTo(entryStream);
                                }
                            }
                        }
                        return File(ms.ToArray(), "application/zip", "documentos.zip");
                    };
                }
                else
                {
                    ViewBag.Mensaje = "Debe seleccionar al menos un documento para iniciar la descarga.";
                    return RedirectToAction("ListarDocumentos");
                }
            }
            catch (Exception e)
            {
                ViewBag.Mensaje = "No se ha podido atender su requerimiento debido a un error del sistema.";
                ViewBag.MensajeError = "Por favor, intente más tarde." + "Error: " + e.Message.ToString();
                return View("ReporteError");
            }
        }

        public List<Documentos> GetDocumentos()
        {
            var result = from d in dbContextPlantilla.Documentos
                         select d;
            if (result.Any())
            {
                return result.ToList();
            }
            else
            {
                return null;
            }
        }

        public Documentos GetDocumentoById(int idDoc)
        {
            var result = from d in dbContextPlantilla.Documentos
                         where d.Id == idDoc
                         select d;
            return result.Any() ? result.First() : null;
        }
    }
}