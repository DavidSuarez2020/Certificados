using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Certificados.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Kernel.Geom;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Image;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using QRCoder;
using iText.Barcodes;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Certificados.Controllers
{
    public class PlantillaController : Controller
    {
        
        private readonly string salt = "certifyYourself";

        // GET: Plantilla
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AdministrarPlantillas()     // administración de plantillas
        {
            return View(GetPlantillasActivas());
        }

        public ActionResult CrearPlantilla()            // creación de un nueva plantilla
        {
            return View(new Plantillas());
        }

        public ActionResult CancelarPlantilla()         // cancela la acción y regresa a Administración
        {
            return RedirectToAction("AdministrarPlantillas");
        }

        public ActionResult VerPlantilla(string nombreArchivoPlantilla)         // cancela la acción y regresa a Administración
        {
            try
            {
                string pathDescarga = Server.MapPath("~/template/");
                byte[] fileBytes = System.IO.File.ReadAllBytes(pathDescarga + nombreArchivoPlantilla);
                return File(fileBytes, "application/pdf", nombreArchivoPlantilla);
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

        public ActionResult DetallarPlantilla(int idPlantilla)         // muestra los detalles de la plantilla
        {
            return View(GetPlantillaById(idPlantilla));
        }

        public ActionResult EditarPlantilla(int idPlantilla)         // edita los detalles de la plantilla
        {
            return View(GetPlantillaById(idPlantilla));
        }

        public ActionResult BorrarPlantilla(int idPlantilla)         // borra los detalles de la plantilla
        {
            return View(GetPlantillaById(idPlantilla));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarBorrado(Plantillas plantilla)         // borra los detalles de la plantilla
        {
            try
            {
                Plantillas plantillaBorrar = new Plantillas();
                using (var dbPlantilla = new ComerciantesEntities())
                {
                    plantillaBorrar = dbPlantilla.Plantillas.Find(plantilla.Id);
                    plantillaBorrar.plantilla_activa = false;
                    dbPlantilla.Entry(plantillaBorrar).State = System.Data.Entity.EntityState.Modified;
                    dbPlantilla.SaveChanges();
                }

                ViewBag.Mensaje = "Plantilla eliminada correctamente.";
                return RedirectToAction("AdministrarPlantillas");
            }
            catch (Exception e)
            {
                ViewBag.Mensaje = "Se ha presentado un problema al intentar eliminar la plantilla.";
                ViewBag.MensajeError = "Error: " + e.Message.ToString();
                return View("ReporteError");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CargarPlantilla(Plantillas plantilla, HttpPostedFileBase file)   // registra plantilla en DB
        {
            ComerciantesEntities dbPlantilla = new ComerciantesEntities();
            if (String.IsNullOrEmpty(plantilla.name))
            {
                ViewBag.Mensaje = "Para crear la plantilla, debe asignarle un nombre.";
                return View("CrearPlantilla", plantilla);
            }

            if (BuscarNombrePlantilla(plantilla.name.Trim()))
            {
                ViewBag.Mensaje = "El nombre " + plantilla.name + " ya existe, escriba otro.";
                return View("CrearPlantilla", plantilla);
            }

            try
            {
                ViewBag.MesajeArchivo = false;
                if (file != null && file.ContentLength > 0)
                {
                    String fileExt = System.IO.Path.GetExtension(file.FileName).ToUpper();

                    // verificar que sea archivo .PDF
                    if (fileExt != ".PDF")
                    {
                        ViewBag.Mensaje = "El archivo cargado no tiene extensión PDF.";
                        return View("CrearPlantilla", plantilla);
                    }

                    // guardar archivo
                    string nombreArchivoPlantilla = System.IO.Path.GetFileName(file.FileName);
                    plantilla.archivo_plantilla = nombreArchivoPlantilla;
                    string _path = System.IO.Path.Combine(Server.MapPath("~/template/"), nombreArchivoPlantilla);
                    file.SaveAs(_path);

                    // validar campos plantilla y archivo cargado
                    var pdfReader = new PdfReader(_path);
                    PdfDocument pdf = new PdfDocument(pdfReader);
                    PdfAcroForm form = PdfAcroForm.GetAcroForm(pdf, true);
                    IDictionary<String, PdfFormField> fields = form.GetFormFields();

                    // verificar campos opcionales con plantilla
                    // campo opciona1
                    if (fields.ContainsKey("opcional1") && !plantilla.opcional1_plantilla)
                    {
                        ViewBag.Mensaje = "No se pudo crear la plantilla porque " +
                            "el archivo contiene el campo opcional1 pero la plantilla no tiene este campo habilitado";
                        pdf.Close();
                        pdfReader.Close();
                        BorrarFile(_path);
                        return View("CrearPlantilla", plantilla);
                    }
                    if (plantilla.opcional1_plantilla && !fields.ContainsKey("opcional1"))
                    {
                        ViewBag.Mensaje = "No se pudo crear la plantilla porque " +
                            "la plantilla tiene el campo opcional1 habilitado pero el archivo no tiene este campo.";
                        pdf.Close();
                        pdfReader.Close();
                        BorrarFile(_path);
                        return View("CrearPlantilla", plantilla);
                    }
                    // campo opcional2
                    if (fields.ContainsKey("opcional2") && !plantilla.opcional2_plantilla)
                    {
                        ViewBag.Mensaje = "No se pudo crear la plantilla porque " +
                            "el archivo contiene el campo opcional2 pero la plantilla no tiene este campo habilitado";
                        pdf.Close();
                        pdfReader.Close();
                        BorrarFile(_path);
                        return View("CrearPlantilla", plantilla);
                    }
                    if (plantilla.opcional2_plantilla && !fields.ContainsKey("opcional2"))
                    {
                        ViewBag.Mensaje = "No se pudo crear la plantilla porque " +
                            "la plantilla tiene el campo opcional2 habilitado pero el archivo no tiene este campo.";
                        pdf.Close();
                        pdfReader.Close();
                        BorrarFile(_path);
                        return View("CrearPlantilla", plantilla);
                    }
                    // campo opcional3
                    if (fields.ContainsKey("opcional3") && !plantilla.opcional3_plantilla)
                    {
                        ViewBag.Mensaje = "No se pudo crear la plantilla porque " +
                            "el archivo contiene el campo opcional3 pero la plantilla no tiene este campo habilitado";
                        pdf.Close();
                        pdfReader.Close();
                        BorrarFile(_path);
                        return View("CrearPlantilla", plantilla);
                    }
                    if (plantilla.opcional3_plantilla && !fields.ContainsKey("opcional3"))
                    {
                        ViewBag.Mensaje = "No se pudo crear la plantilla porque " +
                            "la plantilla tiene el campo opcional3 habilitado pero el archivo no tiene este campo.";
                        pdf.Close();
                        pdfReader.Close();
                        BorrarFile(_path);
                        return View("CrearPlantilla", plantilla);
                    }
                    // campo opcional4
                    if (fields.ContainsKey("opcional4") && !plantilla.opcional4_plantilla)
                    {
                        ViewBag.Mensaje = "No se pudo crear la plantilla porque " +
                            "el archivo contiene el campo opcional4 pero la plantilla no tiene este campo habilitado";
                        pdf.Close();
                        pdfReader.Close();
                        BorrarFile(_path);
                        return View("CrearPlantilla", plantilla);
                    }
                    if (plantilla.opcional3_plantilla && !fields.ContainsKey("opcional4"))
                    {
                        ViewBag.Mensaje = "No se pudo crear la plantilla porque " +
                            "la plantilla tiene el campo opcional4 habilitado pero el archivo no tiene este campo.";
                        pdf.Close();
                        pdfReader.Close();
                        BorrarFile(_path);
                        return View("CrearPlantilla", plantilla);
                    }

                    // asignar valores
                    plantilla.fecha_creacion = DateTime.Now;
                    plantilla.nombres_plantilla = true;
                    plantilla.apellidos_plantilla = true;
                    plantilla.curso_plantilla = true;
                    plantilla.fecha_plantilla = true;
                    plantilla.plantilla_activa = true;
                    plantilla.name = plantilla.name.Trim();

                    // guardar plantilla
                    dbPlantilla.Plantillas.Add(plantilla);
                    if (dbPlantilla.SaveChanges() > 0)
                    {
                        ViewBag.Mensaje = "La plantilla se creó correctamente.";
                        return View(GetPlantillaById(plantilla.Id));
                    }
                    else
                    {
                        ViewBag.Mensaje = "No se pudo guardar la plantilla";
                        return View();
                    }
                }
                else
                {
                    ViewBag.MesajeArchivo = true;
                    ViewBag.Mensaje = "Para crear la plantilla, debe cargar un archivo en formato PDF";
                    return View("CrearPlantilla", plantilla);
                }
            }
            catch (Exception e)
            {
                ViewBag.Mensaje = "Se ha presentado un problema al momento de registrar la plantilla.";
                ViewBag.MensajeError = "Error al intentar guardar la plantilla" + e.Message.ToString();
                return View("ReporteError");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarPlantillaEditada(Plantillas plantilla, HttpPostedFileBase file)   // registra plantilla en DB
        {
            try
            {
                ComerciantesEntities dbPlantilla = new ComerciantesEntities();
                Plantillas plantillaEnEdicion = dbPlantilla.Plantillas.Find(plantilla.Id);
                if (String.IsNullOrEmpty(plantilla.name))
                {
                    ViewBag.Mensaje = "Para guardar los cambios, debe asignarle un nombre a la plantilla.";
                    return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                }

                // verificar modificaciones: name y opcionales
                if (plantillaEnEdicion.name != plantilla.name.Trim())
                {
                    // en caso de cambio de nombre de la plantilla, verificar que no exista ese nombre
                    if (BuscarNombrePlantilla(plantilla.name.Trim()))
                    {
                        ViewBag.Mensaje = "El nombre " + plantilla.name + " ya existe, escriba otro.";
                        return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                    }
                    plantillaEnEdicion.name = plantilla.name.Trim();
                }
                if (plantillaEnEdicion.opcional1_plantilla != plantilla.opcional1_plantilla)
                {
                    plantillaEnEdicion.opcional1_plantilla = plantilla.opcional1_plantilla;
                }
                if (plantillaEnEdicion.opcional2_plantilla != plantilla.opcional2_plantilla)
                {
                    plantillaEnEdicion.opcional2_plantilla = plantilla.opcional2_plantilla;
                }
                if (plantillaEnEdicion.opcional3_plantilla != plantilla.opcional3_plantilla)
                {
                    plantillaEnEdicion.opcional3_plantilla = plantilla.opcional3_plantilla;
                }
                if (plantillaEnEdicion.opcional4_plantilla != plantilla.opcional4_plantilla)
                {
                    plantillaEnEdicion.opcional4_plantilla = plantilla.opcional4_plantilla;
                }

                // obtener el path del archivo para validación de los campos de la plantilla
                string _path = System.IO.Path.Combine(Server.MapPath("~/template/"), plantillaEnEdicion.archivo_plantilla);

                // revisar si se cambió el archivo de la plantilla
                if (file != null && file.ContentLength > 0)
                {
                    String fileExt = System.IO.Path.GetExtension(file.FileName).ToUpper();

                    // verificar que sea archivo .PDF
                    if (fileExt != ".PDF")
                    {
                        ViewBag.Mensaje = "El archivo cargado no tiene extensión PDF.";
                        return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                    }

                    // guardar el nuevo archivo
                    string nombreArchivoPlantilla = System.IO.Path.GetFileName(file.FileName);
                    plantillaEnEdicion.archivo_plantilla = nombreArchivoPlantilla;
                    _path = System.IO.Path.Combine(Server.MapPath("~/template/"), nombreArchivoPlantilla);
                    file.SaveAs(_path);
                }

                // validar campos plantilla y archivo cargado
                var pdfReader = new PdfReader(_path);
                PdfDocument pdf = new PdfDocument(pdfReader);
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdf, true);
                IDictionary<String, PdfFormField> fields = form.GetFormFields();
                // campo opciona1
                if (fields.ContainsKey("opcional1") && !plantilla.opcional1_plantilla)
                {
                    ViewBag.Mensaje = "No se pudo guardar los cambios porque " +
                        "el archivo contiene el campo opcional1 pero la plantilla no tiene este campo habilitado";
                    return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                }
                if (plantilla.opcional1_plantilla && !fields.ContainsKey("opcional1"))
                {
                    ViewBag.Mensaje = "No se pudo guardar los cambios porque " +
                        "la plantilla tiene el campo opcional1 habilitado pero el archivo no tiene este campo.";
                    return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                }
                // campo opcional2
                if (fields.ContainsKey("opcional2") && !plantilla.opcional2_plantilla)
                {
                    ViewBag.Mensaje = "No se pudo guardar los cambios porque " +
                        "el archivo contiene el campo opcional2 pero la plantilla no tiene este campo habilitado";
                    return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                }
                if (plantilla.opcional2_plantilla && !fields.ContainsKey("opcional2"))
                {
                    ViewBag.Mensaje = "No se pudo guardar los cambios porque " +
                        "la plantilla tiene el campo opcional2 habilitado pero el archivo no tiene este campo.";
                    return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                }
                // campo opcional3
                if (fields.ContainsKey("opcional3") && !plantilla.opcional3_plantilla)
                {
                    ViewBag.Mensaje = "No se pudo guardar los cambios porque " +
                        "el archivo contiene el campo opcional3 pero la plantilla no tiene este campo habilitado";
                    return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                }
                if (plantilla.opcional3_plantilla && !fields.ContainsKey("opcional3"))
                {
                    ViewBag.Mensaje = "No se pudo guardar los cambios porque " +
                        "la plantilla tiene el campo opcional3 habilitado pero el archivo no tiene este campo.";
                    return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                }
                // campo opcional4
                if (fields.ContainsKey("opcional4") && !plantilla.opcional4_plantilla)
                {
                    ViewBag.Mensaje = "No se pudo guardar los cambios porque " +
                        "el archivo contiene el campo opcional4 pero la plantilla no tiene este campo habilitado";
                    return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                }
                if (plantilla.opcional4_plantilla && !fields.ContainsKey("opcional4"))
                {
                    ViewBag.Mensaje = "No se pudo guardar los cambios porque " +
                        "la plantilla tiene el campo opcional4 habilitado pero el archivo no tiene este campo.";
                    return View("EditarPlantilla", GetPlantillaById(plantilla.Id));
                }
                pdf.Close();
                pdfReader.Close();

                // guardar cambios
                dbPlantilla.Entry(plantillaEnEdicion).State = System.Data.Entity.EntityState.Modified;

                // guardar plantilla
                if (dbPlantilla.SaveChanges() > 0)
                {
                    ViewBag.Mensaje = "Se guardaron los cambios correctamente.";
                    return View("CargarPlantilla", GetPlantillaById(plantilla.Id));
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo guardar los cambios. Intente más tarde.";
                    return View();
                }
            }
            catch (Exception e)
            {
                ViewBag.Mensaje = "Se ha presentado un problema al momento de guardar los cambios.";
                ViewBag.MensajeError = "Error: " + e.Message.ToString();
                return View("ReporteError");
            }
        }


        public ActionResult SeleccionarDatos(Plantillas plantilla)       // seleccionar datos para la plantilla
        {
            return View(plantilla);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CargarDatos(Plantillas plantilla, HttpPostedFileBase file)    // carga de datos y generación documentos
        {
            // obtener el objeto plantilla en caso que plantilla sea null
            if (String.IsNullOrEmpty(plantilla.name))
            {
                plantilla = GetPlantillaById(plantilla.Id);
            }

            Plantilla_Dato pdTemp;
            List<Plantilla_Dato> listaPDGuardada = new List<Plantilla_Dato>();
            ComerciantesEntities dbGuardarRegistros = new ComerciantesEntities();
            ViewBag.Mensaje = String.Empty;

            // registra en DB
            try
            {
                // verificar que exista un archivo y sea en formato .CSV
                if (file != null && file.ContentLength > 0)
                {
                    String fileExt = System.IO.Path.GetExtension(file.FileName).ToUpper();

                    // verificar que sea archivo .CSV
                    if (fileExt != ".CSV")
                    {
                        ViewBag.Mensaje = "El archivo cargado no tiene extensión CSV.";
                        return View("SeleccionarDatos", plantilla);
                    }

                    // guardar archivo CVS - temporal
                    string nombreArchivoCVS = System.IO.Path.GetFileName(file.FileName);
                    string pathCVS = System.IO.Path.Combine(Server.MapPath("~/Uploads/"), nombreArchivoCVS);
                    file.SaveAs(pathCVS);

                    // lectura de datos del archivo CVS
                    List<Datos> listaDatosRegistrar = new List<Datos>();
                    string cvsData = System.IO.File.ReadAllText(pathCVS);
                    string[] cvsDataArray = cvsData.Split('\n');
                    for (int i = 1; i < cvsDataArray.Length; i++)
                    {
                        if (!String.IsNullOrEmpty(cvsDataArray[i]))
                        {
                            string[] datoCVS = cvsDataArray[i].Split(',');
                            listaDatosRegistrar.Add(new Datos()
                            {
                                nombres = datoCVS[1],
                                apellidos = datoCVS[2],
                                curso = datoCVS[3],
                                fecha = datoCVS[4],
                                opcional1 = datoCVS[5],
                                opcional2 = datoCVS[6],
                                opcional3 = datoCVS[7],
                                opcional4 = datoCVS[8]
                            });
                        }
                    }

                    // guardar en tabla DATOS y PLANTILLA_DATO
                    foreach (Datos dato in listaDatosRegistrar)
                    {
                        // DATO: verificar campos opcionales
                        dato.opcional1 = !plantilla.opcional1_plantilla ? "0" : dato.opcional1;
                        dato.opcional2 = !plantilla.opcional2_plantilla ? "0" : dato.opcional2;
                        dato.opcional3 = !plantilla.opcional3_plantilla ? "0" : dato.opcional3;
                        dato.opcional4 = !plantilla.opcional4_plantilla ? "0" : dato.opcional4;
                        dato.fecha_registro = DateTime.Now;
                        dbGuardarRegistros.Datos.Add(dato);

                        // DOCUMENTO: generar pdf y registrar objeto
                        // path template
                        string archivoPath = "~/template/";
                        string nombreArchivoPlantilla = plantilla.archivo_plantilla;

                        // abrir plantilla
                        var pdfReader = new PdfReader(Request.MapPath(archivoPath + nombreArchivoPlantilla));

                        // buffer para almacenar
                        MemoryStream ms = new MemoryStream();
                        PdfWriter pw = new PdfWriter(ms);
                        PdfDocument pdf = new PdfDocument(pdfReader, pw);
                        Document doc = new Document(pdf, PageSize.A4);

                        // mapeo de datos - campos obligatorios
                        PdfAcroForm form = PdfAcroForm.GetAcroForm(pdf, true);
                        IDictionary<String, PdfFormField> fields = form.GetFormFields();

                        // verificar campos opcionales con datos
                        if (fields.ContainsKey("opcional1") && dato.opcional1 == "0")
                        {
                            ViewBag.Mensaje = "Se ha producido un error al momento de cargar los datos y generar el documento";
                            ViewBag.MensajeError = "La plantilla tiene el campo opcional1 habilitado pero no se cargan datos para este campo";
                            return View("ReporteError");
                        }

                        fields.TryGetValue("nombres", out PdfFormField toSet);
                        toSet.SetValue(dato.nombres.ToString());
                        fields.TryGetValue("apellidos", out toSet);
                        toSet.SetValue(dato.apellidos.ToString());
                        fields.TryGetValue("curso", out toSet);
                        toSet.SetValue(dato.curso.ToString());
                        fields.TryGetValue("fecha", out toSet);
                        toSet.SetValue(dato.fecha.ToString());

                        // campos opcionales
                        if (plantilla.opcional1_plantilla)
                        {
                            fields.TryGetValue("opcional1", out toSet);
                            toSet.SetValue(dato.opcional1.ToString());
                        }
                        if (plantilla.opcional2_plantilla)
                        {
                            fields.TryGetValue("opcional2", out toSet);
                            toSet.SetValue(dato.opcional2.ToString());
                        }
                        if (plantilla.opcional3_plantilla)
                        {
                            fields.TryGetValue("opcional3", out toSet);
                            toSet.SetValue(dato.opcional3.ToString());
                        }
                        if (plantilla.opcional4_plantilla)
                        {
                            fields.TryGetValue("opcional4", out toSet);
                            toSet.SetValue(dato.opcional4.ToString());
                        }
                        form.FlattenFields();

                        // generar y agregar código QR
                        ImageData imageData = ImageDataFactory.CreatePng(GenerarQRByte(dato.nombres));
                        iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData);
                        var pageWidth = pdf.GetDefaultPageSize().GetWidth();
                        var pageHeight = pdf.GetDefaultPageSize().GetHeight();
                        image.SetHeight(140f);
                        image.SetWidth(140f);
                        image.SetFixedPosition(pageWidth - 140f, 20f);
                        doc.Add(image);
                        doc.Close();

                        // guardar bytes
                        byte[] byteStream = ms.ToArray();
                        ms = new MemoryStream();
                        ms.Write(byteStream, 0, byteStream.Length);
                        ms.Position = 0;

                        // guardar en servidor
                        string nombreDocumento = dato.nombres + dato.apellidos + "_" + DateTime.Now.GetHashCode().ToString().Replace("-", "") + ".pdf";
                        string destinoDocumento = Server.MapPath("~/template_certificados/");
                        FileStream fileStream = new FileStream(destinoDocumento + nombreDocumento, FileMode.Create, FileAccess.ReadWrite);
                        ms.WriteTo(fileStream);
                        fileStream.Close();

                        // registrar Documento
                        Documentos docTemp = new Documentos()
                        {
                            dato_id = dato.Id,
                            fecha_generado = DateTime.Today,
                            ruta_archivo = nombreDocumento,
                            codigo_verificacion = CreateHashString(nombreDocumento)
                        };
                        dbGuardarRegistros.Documentos.Add(docTemp);

                        // PLANTILLA_DATO
                        pdTemp = new Plantilla_Dato()
                        {
                            plantilla_id = plantilla.Id,
                            dato_id = dato.Id
                        };
                        dbGuardarRegistros.Plantilla_Dato.Add(pdTemp);

                        // guardar registros en las tablas
                        dbGuardarRegistros.SaveChanges();
                        listaPDGuardada.Add(GetPDByDatoId(pdTemp.dato_id));
                    }

                    // respuesta a Vista
                    ViewBag.CantidadDatos = listaPDGuardada.Count();
                    string datosIdString = String.Empty;
                    if (listaPDGuardada.Count() > 1)
                    {
                        foreach (Plantilla_Dato pd in listaPDGuardada)
                        {
                            datosIdString = datosIdString + pd.dato_id.ToString() + ",";
                        }
                    }

                    // eliminar archivo CVS cargado
                    System.IO.File.Delete(pathCVS);

                    ViewBag.NombreArchivo = GetDocumentoByDatoId(listaPDGuardada.ElementAt(0).dato_id).ruta_archivo;
                    ViewBag.DatosIdString = datosIdString;
                    return View(plantilla);
                }
                else
                {
                    ViewBag.Mensaje = "Para crear la plantilla, debe cargar un archivo en formato CVS";
                    return View("SeleccionarDatos", plantilla);
                }
            }
            catch (Exception e)
            {
                ViewBag.Mensaje = "Se ha producido un error al momento de cargar los datos y generar el documento";
                ViewBag.MensajeError = "Error producido" + e.Message.ToString();
                return View("ReporteError");
            }
        }

        public ActionResult DescargarDocumento(string nombreArchivo)        // descarga individual de DOCUMENTO generado
        {
            string pathDescarga = Server.MapPath("~/template_certificados/");
            byte[] fileBytes = System.IO.File.ReadAllBytes(pathDescarga + nombreArchivo);
            return File(fileBytes, "application/pdf", nombreArchivo);
        }

        public ActionResult DescargarZipDocumentos(string datosIdString)    // descarga ZIP de documentos generados
        {
            // obtener lista de objetos documentos generados
            string[] datosIdArray = datosIdString.Split(',');
            List<Documentos> listaDocumentos = new List<Documentos>();
            foreach (string datoIdString in datosIdArray)
            {
                if (!String.IsNullOrEmpty(datoIdString) || !String.IsNullOrWhiteSpace(datoIdString))
                {
                    if (Int32.TryParse(datoIdString, out int datoId))
                    {
                        listaDocumentos.Add(GetDocumentoByDatoId(datoId));
                    }
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
        
        public List<Plantillas> GetPlantillasActivas()
        {
            ComerciantesEntities dbPlantilla = new ComerciantesEntities();
            var result = from p in dbPlantilla.Plantillas
                         where p.plantilla_activa == true
                         select p;
            if (result.Any())
            {
                return result.ToList();
            }
            else
            {
                return null;
            }
        }

        public Plantillas GetPlantillaById(int plantilla_id)
        {
            ComerciantesEntities dbPlantilla = new ComerciantesEntities();
            var result = from p in dbPlantilla.Plantillas
                         where p.Id == plantilla_id
                         select p;
            if (result.Any())
            {
                return result.First();
            }
            else
            {
                return null;
            }
        }

        public List<Datos> GetDatos()
        {
            ComerciantesEntities dbDatos = new ComerciantesEntities();
            var result = from d in dbDatos.Datos
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

        public bool VerificarDatoById(int id)
        {
            ComerciantesEntities dbDatos = new ComerciantesEntities();
            var result = from d in dbDatos.Plantilla_Dato
                         where d.dato_id == id
                         select d;
            return result.Any();
        }

        public Datos GetDatoById(int idDato)
        {
            ComerciantesEntities dbDatos = new ComerciantesEntities();
            var result = from d in dbDatos.Datos
                         where d.Id == idDato
                         select d;
            if (result.Any())
            {
                return result.First();
            }
            else
            {
                return null;
            }
        }

        public List<Plantilla_Dato> GetPDByPlantillaId(int id)
        {
            ComerciantesEntities dbPD = new ComerciantesEntities();
            var result = from pd in dbPD.Plantilla_Dato
                         where pd.plantilla_id == id
                         select pd;
            if (result.Any())
            {
                return result.ToList();
            }
            else
            {
                return null;
            }
        }

        public Plantilla_Dato GetPDByDatoId(int id)
        {
            ComerciantesEntities dbPD = new ComerciantesEntities();
            var result = from pd in dbPD.Plantilla_Dato
                         where pd.dato_id == id
                         select pd;
            if (result.Any())
            {
                return result.First();
            }
            else
            {
                return null;
            }
        }

        public Plantilla_Dato GetPDById(int id)
        {
            ComerciantesEntities dbPD = new ComerciantesEntities();
            var result = from pd in dbPD.Plantilla_Dato
                         where pd.Id == id
                         select pd;
            if (result.Any())
            {
                return result.First();
            }
            else
            {
                return null;
            }
        }

        public Plantillas GetPlantillaByDatoId(int idDato)
        {
            ComerciantesEntities dbPD = new ComerciantesEntities();
            var result = from pd in dbPD.Plantilla_Dato
                         where pd.dato_id == idDato
                         select pd.plantilla_id;
            if (result.Any())
            {
                return GetPlantillaById(result.First());
            }
            else
            {
                return null;
            }
        }

        public List<Plantilla_Dato> GetPDByDatosId(List<Datos> listaDatos)
        {
            ComerciantesEntities dbPD = new ComerciantesEntities();
            List<Plantilla_Dato> listaPD = new List<Plantilla_Dato>();
            foreach (Datos dato in listaDatos)
            {
                var result = from pd in dbPD.Plantilla_Dato
                             where pd.dato_id == dato.Id
                             select pd;
                if (result.Any())
                {
                    listaPD.Add(result.First());
                }
            }
            return listaPD;
        }

        public Documentos GetDocumentoByDatoId(int idDato)
        {
            ComerciantesEntities dbDoc = new ComerciantesEntities();
            var result = from doc in dbDoc.Documentos
                         where doc.dato_id == idDato
                         select doc;
            if (result.Any())
            {
                return result.First();
            }
            else
            {
                return null;
            }
        }

        public bool BuscarNombrePlantilla(string nombrePlantilla)
        {
            ComerciantesEntities dbPlantilla = new ComerciantesEntities();
            var result = from p in dbPlantilla.Plantillas
                         where p.name == nombrePlantilla
                         select p;
            return result.Any();
        }

        public byte[] GenerarQRByte(string hashText)   // generar código QR con información del Comerciante
        {
            // información del comerciantes que irá en el código QR
            //string dataForQR = $"https://localhost:44320/Comerciantes/ValidarCertificado/{hashText}";
            string dataForQR = hashText;

            // generar código QR - información
            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(dataForQR, QRCodeGenerator.ECCLevel.Q);

            // renderizar - representación
            PngByteQRCode qrCode = new PngByteQRCode(qRCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            //return Convert.ToBase64String(qrCodeImage);   para devolver como string
            return qrCodeImage;
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
                    return BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                }
            }
            else
            {
                return String.Empty;
            }
        }

        public void BorrarFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}