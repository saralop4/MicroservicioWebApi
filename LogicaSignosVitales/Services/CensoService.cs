using DataCenso.DTOs;
using DataSignosVitales.DTOs;
using DataSignosVitales.Entities.NotaEnfermeriaModels;
using DataSignosVitales.Interfaces;
using LogicaSignosVitales.Exceptions;
using LogicaSignosVitales.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;


namespace LogicaSignosVitales.Services
{
    public class CensoService : ICensoService
    {
        private readonly INotaEnfermeriaDbContext _notaenfermeriadbcontext;

        public CensoService(INotaEnfermeriaDbContext notaenfermeriadbcontext)
        {
            _notaenfermeriadbcontext = notaenfermeriadbcontext;
        }

        public async Task<List<SisCamaDTOs>> MostrarCamas(int? pabellon)
        {

            bool pabellonExistente = _notaenfermeriadbcontext.SisCamas.Any(u => u.Pabellon == pabellon);
            if (pabellonExistente)
            {

                var numeroMax = _notaenfermeriadbcontext.SisCamas
                    .Where(sc => sc.Pabellon == pabellon)
                    .SelectMany(sc => _notaenfermeriadbcontext.NotasEnfermeria
                        .Where(ne => ne.Ingreso == sc.EstudioPaciente)
                        .DefaultIfEmpty(),
                        (sc, ne) => new { sc.EstudioPaciente, sc.Codigo, ne.Numero })
                    .GroupBy(x => new { x.EstudioPaciente, x.Codigo })
                    .Select(g => new
                    {
                        g.Key.EstudioPaciente,
                        g.Key.Codigo,
                        NumeroMax = g.Max(x => x.Numero)
                    });

                var mainQuery = _notaenfermeriadbcontext.SisCamas
                    .Where(sc => sc.Pabellon == pabellon)
                    .GroupJoin(_notaenfermeriadbcontext.NotasEnfermeria,
                        sc => sc.EstudioPaciente,
                        ne => ne.Ingreso,
                        (sc, ne) => new { SisCama = sc, NotasEnfermeria = ne })
                    .SelectMany(
                        x => x.NotasEnfermeria.DefaultIfEmpty(),
                        (x, ne) => new { x.SisCama, NotasEnfermeria = ne })
                    .Join(numeroMax,
                        sc => new { sc.SisCama.EstudioPaciente, sc.SisCama.Codigo },
                        nm => new { nm.EstudioPaciente, nm.Codigo },
                        (sc, nm) => new
                        {
                            sc.SisCama.EstudioPaciente,
                            sc.SisCama.Codigo
                        });

                var unionQuery = _notaenfermeriadbcontext.SisCamas
                        .Where(sc => sc.Pabellon == pabellon && (sc.EstudioPaciente == null || sc.EstudioPaciente == -1))
                        .Select(sc => new
                        {
                            sc.EstudioPaciente,
                            sc.Codigo
                        });

                var finalQuery = mainQuery.Union(unionQuery);


                var listaSinValidar = await finalQuery.ToListAsync();
                var listaConValidacion = new List<SisCamaDTOs>();



                foreach (var item in listaSinValidar)
                {


                    if (item.EstudioPaciente != null && item.EstudioPaciente.Value != -1)
                    {

                        (string? color, string? observacion) = await ValidarEstadoConciencia(item.EstudioPaciente.Value);

                        listaConValidacion.Add(new SisCamaDTOs
                        {
                            Estudio = (int)item.EstudioPaciente,
                            Nro_Cama = item.Codigo,
                            Color = color,
                            Observacion = observacion
                        });
                    }
                    else
                    {
                        listaConValidacion.Add(new SisCamaDTOs
                        {
                            Estudio = (int)(item.EstudioPaciente ?? -1),
                            Nro_Cama = item.Codigo,
                            Color = "No aplicable",
                            Observacion = "No aplicable"
                        });
                    }
                }

                if (listaConValidacion != null)
                {
                    return listaConValidacion;
                }

                throw new ValidationListaConValidacionNullReferenceException();

            }

            throw new ValidationArgumentsEntityNullException();
        }



        private async Task<(string?, string?)> ValidarEstadoConciencia(int estudioPaciente)
        {
          
            
            var result = await ConsultarLosSignosVitales(estudioPaciente);


            //Console.WriteLine(estudioPaciente);
            //Console.WriteLine(result.TensionArterial);

            if ( result.TensionArterial == "-1")
            {
                string vacio = "No aplicable";                
                return (vacio, vacio);
            };

            int sumatoria = 0;
            string? cadenaOriginal = result.TensionArterial;
            char separador = '/';

            string[] subcadena = cadenaOriginal.Split(separador);

            for (int i = 0; i < subcadena.Length; i++)
            {
                if (int.TryParse(subcadena[i], out int taValue))
                {
                    if (i == 0) // PAS
                    {
                        sumatoria +=
                            (taValue < 80) ? 3 :
                            (taValue >= 80 && taValue <= 89) ? 2 :
                            (taValue >= 90 && taValue <= 139) ? 0 :
                            (taValue >= 140 && taValue <= 149) ? 1 :
                            (taValue >= 150 && taValue <= 159) ? 2 :
                            (taValue >= 160) ? 3 : 0;
                    }
                    else if (i == 1) // PAD
                    {
                        sumatoria +=
                            (taValue < 90) ? 0 :
                            (taValue >= 90 && taValue <= 99) ? 1 :
                            (taValue >= 100 && taValue <= 109) ? 2 :
                            (taValue >= 110) ? 3 : 0;
                    }
                }
            }

            if (int.TryParse(result.FrecuenciaRespiratoria, out int frValue))
            {
                sumatoria +=
                    (frValue < 10) ? 3 :
                    (frValue >= 10 && frValue <= 17) ? 0 :
                    (frValue >= 18 && frValue <= 24) ? 1 :
                    (frValue >= 25 && frValue <= 29) ? 2 :
                    (frValue >= 30) ? 3 : 0;
            }

            if (int.TryParse(result.FrecuenciaCardiaca, out int fcValue))
            {
                sumatoria +=
                    (fcValue < 60) ? 3 :
                    (fcValue >= 60 && fcValue <= 110) ? 0 :
                    (fcValue >= 111 && fcValue <= 149) ? 2 :
                    (fcValue >= 150) ? 3 : 0;
            }


            if (result.Oxigeno.Equals("Aire ambiente", StringComparison.OrdinalIgnoreCase))
            {
               sumatoria += 0;
            }
            else if (int.TryParse(result.Oxigeno, out int o2Value))
            {
                sumatoria +=
                    (o2Value >= 24 && o2Value <= 39) ? 1 :
                    (o2Value >= 40) ? 3 : 0;
            }

            double? tpValue = (double?)result.Temperatura;
            if (tpValue.HasValue)
            {
                sumatoria +=
                    (tpValue < 34.0) ? 3 :
                    (tpValue >= 34.0 && tpValue <= 35.0) ? 1 :
                    (tpValue >= 35.1 && tpValue <= 37.9) ? 0 :
                    (tpValue >= 38.0 && tpValue <= 38.9) ? 1 :
                    (tpValue >= 39) ? 3 : 0;
            }

            sumatoria += (bool)(result.EstadoConciencia ?? false) ? 3 : 0;


            string mensajeAlerta;
            string color;
            if (sumatoria >= 6)
            {
                color = "Rojo";
                mensajeAlerta = "Monitoreo continuo de signos vitales " +
                    " LLAMADO - Emergente al equipo con competencias en el diagnostico";
            }
            else if (sumatoria >= 4 && sumatoria < 6)
            {
                color = "Naranja";
                mensajeAlerta = "Minimo cada hora " +
                    " LLAMADO - Urgente al equipo medico a cargo de la paciente y al personal " +
                    "con las competencias para manejo de la enfermedad aguda.";
            }
            else if (sumatoria >= 1 && sumatoria <= 3)
            {
                color = "Amarillo";
                mensajeAlerta = "Minimo cada 4 horas " +
                    " LLAMADO - Enfermera a cargo";
            }
            else
            {
                color = "Blanco";
                mensajeAlerta = "OBSERVACION DE RUTINA";
            }



            return (color, mensajeAlerta);
        }


        private async Task<NotaEnfermeriaDTOs?> ConsultarLosSignosVitales(int estudioPaciente)
        {
      
            var notas = _notaenfermeriadbcontext.NotasEnfermeria
                   .Where(n => n.Ingreso == estudioPaciente);                   


            if (notas.Count() == 0)
            {

                return new NotaEnfermeriaDTOs
                    {
                        TensionArterial = "-1",
                        FrecuenciaCardiaca = "0",
                        FrecuenciaRespiratoria = "0",
                        Temperatura = 0M,
                        Oxigeno = "0",
                        EstadoConciencia = false
                };
            }


            var numeroMaximo = await notas.MaxAsync(n => n.Numero);

            if (numeroMaximo == null)
            {
                return null;
            }


            var ultimaNota = await notas
                    .Where(n => n.Numero == numeroMaximo )
                    .Select(n => new NotaEnfermeriaDTOs
                    {
                        TensionArterial = n.Ta,
                        FrecuenciaCardiaca = n.Fc,
                        FrecuenciaRespiratoria = n.Fr,
                        Temperatura = n.Tp,
                        Oxigeno = n.O2,
                        EstadoConciencia = n.EstadoConciencia ?? false,
                    })
                    .FirstOrDefaultAsync();
    
            return ultimaNota ?? new NotaEnfermeriaDTOs
                {
                    TensionArterial = "-1",
                    FrecuenciaCardiaca = "0",
                    FrecuenciaRespiratoria = "0",
                    Temperatura = 0M,
                    Oxigeno = "0",
                    EstadoConciencia = false
                };
            }

    }
}
