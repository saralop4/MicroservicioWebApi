using DataSignosVitales.DTOs;
using DataSignosVitales.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSignosVitales.Entities.NotaEnfermeriaModels;

namespace LogicaSignosVitales.Interfaces
{
    public  interface INotaEnfermeriaService
    {
        Task<NotaEnfermeriaDTOs> ObtenerSignoVital(string? numero, int? estudio);

        Task<NotaEnfermeriaDTOs> CrearSignoVital(NotaEnfermeriaDTOs notaEnfermeriaDtos);

        Task ActualizarSignoVital(string? numero, NotaEnfermeriaDTOs notaEnfermeriaDtos);

    }
}

