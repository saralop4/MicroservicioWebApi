using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataCenso.DTOs
{
    public class SisCamaDTOs
    {
       

        [JsonIgnore]
        public int Pabellon { get; set; }
        public int Estudio { get; set; }
        public int Nro_Cama { get; set; }
        [JsonIgnore]
        public DateTime? Fecha { get; set; }
        public string? Color { get; set; }
        public string? Observacion { get; set; }


    }
}
