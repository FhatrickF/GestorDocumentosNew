using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mvc4.Models
{
    public class DocumentoViewModel
    {
        public int IdDocumento { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int? Version { get; set; }
        public bool? VersionFinal { get; set; }
        public bool EsBorrador { get; set; }
        public string Texto { get; set; }
    }

    public class NuevoDocumentViewModel
    {
        public string IdDocumento { get; set; }
        [Required]
        public string Titulo { get; set; }
        [Required]
        public string Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int Version { get; set; }
        public bool VersionFinal { get; set; }
        public bool EsBorrador { get; set; }
        [AllowHtml]
        public string Texto { get; set; }
    }

    public class VersionesDocumento
    {
        public string id { get; set; }
        public string nombre { get; set; }
    }

    public class DetalleDocumento
    {
        public NuevoDocumentViewModel Document { get; set; }
        public List<VersionesDocumento> ListaVersiones { get; set; }
    }

    public class MedioAmbiental
    {
        public string id { get; set; }
        public string IdDocumento { get; set; }
        public string Seccion { get; set; }
        public DateTime Fecha { get; set; }
        public string Numero { get; set; }
        public string Norma { get; set; }
        [Required]
        public string Organismo { get; set; }
        public string SubOrganismo { get; set; }
        public string Categoria { get; set; }
        public string Tema { get; set; }
        [Required]
        public string Titulo { get; set; }
        [AllowHtml]
        [Required]
        public string Texto { get; set; }
        public bool EsBorrador { get; set; }
        public List<VersionesDocumento> Versiones { get; set; }
    }

    public class FormularioBusqueda
    {
        public string Ct { get; set; }
        public string Lr { get; set; }
        public string Lt { get; set; }
        public string Lzf { get; set; }
        public string Liva { get; set; }
        public string Circular { get; set; }
        public string Decreto { get; set; }
        public string Dfl { get; set; }
        public string Dl { get; set; }
        public string Ds { get; set; }
        public string Ley { get; set; }
        public string Resolucion { get; set; }
        public string Fecha { get; set; }
        public string Numero { get; set; }
        public string Articulo { get; set; }
        public string Inciso { get; set; }
        public string Texto { get; set; }
        public string Coleccion { get; set; }
        public int Pagina { get; set; }
    }
}