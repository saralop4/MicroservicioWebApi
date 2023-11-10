using DataSignosVitales.Entities.NotaEnfermeriaModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace DataSignosVitales.DTOs
{
    public class NotaEnfermeriaDTOs
    {

        [Required(ErrorMessage = "El campo numero_SignoVital es requerido.")]
        [MaxLength(10, ErrorMessage = " longitud maxima permita de 10")]
        public string Numero_SignoVital { get; set; } = null!;

        public int Estudio { get; set; }

        public DateTime? Fecha { get; set; }

        public string? Hora { get; set; }

        public string? Nota { get; set; } = "";

        public string? Enfermera { get; set; }

        [MaxLength(10, ErrorMessage = "longitud maxima permita de 10 caracteres")]
        public string? CodigoEnfermera { get; set; }

        [MaxLength(10, ErrorMessage = "longitud maxima permita de 10 caracteres")]
        public string? TensionArterial { get; set; }

        [MaxLength(10, ErrorMessage = "longitud maxima permita de 10 caracteres")]
        public string? FrecuenciaCardiaca { get; set; }

        [MaxLength(10, ErrorMessage = "longitud maxima permita de 10 caracteres")]
        public string? FrecuenciaRespiratoria { get; set; }

        [MaxLength(10, ErrorMessage = "longitud maxima permita de 10 caracteres")]
        public string? Peso { get; set; }

        public decimal? Temperatura { get; set; }

        [MaxLength(10, ErrorMessage = "longitud maxima permita de 10 caracteres")]
        public string? Oxigeno { get; set; }

        [JsonIgnore]
        public int? SoloVitales { get; set; } = 1;

        [JsonIgnore]
        public bool Cerrada { get; set; } = false;
        public decimal? Glucometria { get; set; }

        public int? UnidadFuncional { get; set; }

        [MaxLength(10, ErrorMessage = "longitud maxima permita de 10 caracteres")]
        public string? Tamizaje { get; set; }

        public bool? EstadoConciencia { get; set; }




    }
}
