using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Mvc;

namespace GestorDocumentosEntities
{
    public class sgd_documentoEntity
    {
        public string id { get; set; }
        public string IdDocumento { get; set; }
        public int Orden { get; set; }
        public string Coleccion { get; set; }
        public int Anio { get; set; }
        public string AnioC { get; set; }
        public string Apendice { get; set; }
        public string Articulo { get; set; }
        public string AplicaArticulo { get; set; }
        public string Categoria { get; set; }
        public string Comentario { get; set; }
        public string Cve { get; set; }
        public DateTime Fecha { get; set; }
        public string Iddo { get; set; }
        public string IdRep { get; set; }
        public string Inciso { get; set; }
        public string Minred { get; set; }
        public string Nompop { get; set; }
        [Required]
        public string Norma { get; set; }
        public string AplicaNorma { get; set; }
        public string Numero { get; set; }
        public string AplicaNumero { get; set; }
        public string Organismo { get; set; }
        public string OrganismoUno { get; set; }
        public string Regco { get; set; }
        public string Resuel { get; set; }
        public string Rol { get; set; }
        public string Seccion { get; set; }
        public string Suborganismo { get; set; }
        public string Tema { get; set; }
        public string Temas { get; set; }
        [Required]
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public Nullable<System.DateTime> FechaCreacion { get; set; }
        public int Version { get; set; }
        public bool VersionFinal { get; set; }
        public bool EsBorrador { get; set; }
        [AllowHtml]
        [Required]
        public string Texto { get; set; }
        public List<VersionesDocumentoEntity> Versiones { get; set; }
        public List<LinkEntity> Links { get; set; }
    }
}
