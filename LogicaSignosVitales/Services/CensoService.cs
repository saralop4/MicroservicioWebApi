using DataCenso.DTOs;
using DataSignosVitales.DTOs;
using DataSignosVitales.Interfaces;
using LogicaSignosVitales.Exceptions;
using LogicaSignosVitales.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


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

            if (pabellon >= 1 && pabellon != null) {

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
                if (item.EstudioPaciente.HasValue && item.EstudioPaciente.Value != -1)
                {
                     (string color,string observacion) = await ValidarEstadoConciencia(item.EstudioPaciente.Value);

                    listaConValidacion.Add(new SisCamaDTOs
                    {
                        Estudio = item.EstudioPaciente.Value,
                        Nro_Cama = item.Codigo,
                        Color = color,
                        Observacion = observacion
                    });
                }
                else
                {
                    listaConValidacion.Add(new SisCamaDTOs
                    {
                        Estudio = item.EstudioPaciente ?? default(int),
                        Nro_Cama = item.Codigo,
                        Color = "No aplicable",
                        Observacion = "No aplicable"
                    });
                }
            }

            return listaConValidacion;
            }

            throw new ValidationPabellonNullException();
        }



        private async Task<(string?, string?)> ValidarEstadoConciencia(int estudioPaciente)
        {

            await ValidarEstudioSisMae(estudioPaciente);

            var result = await ValidarEstudioSignosVitales(estudioPaciente);

            if (result == null)
            {
                throw new ValidationEstudioException("error: no se encuentra el registro");
            }

            int sumatoria = 0;
            string? cadenaOriginal = result.Tamizaje;
            char separador = '/';

            string[] subcadena = cadenaOriginal.Split(separador);

            for (int i = 0; i < subcadena.Length; i++)
            {
                if (int.TryParse(subcadena[i], out int taValue))
                {
                    if (i == 0)
                    {
                        sumatoria +=
                            (taValue < 80) ? 3 :
                            (taValue >= 80 && taValue <= 89) ? 2 :
                            (taValue >= 90 && taValue <= 139) ? 0 :
                            (taValue >= 140 && taValue <= 149) ? 1 :
                            (taValue >= 150 && taValue <= 159) ? 2 :
                            (taValue >= 160) ? 3 : 0;
                    }
                    else if (i == 1)
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
                    (frValue >= 25 && frValue <= 29) ? 2 : 3;
            }

            if (int.TryParse(result.FrecuenciaCardiaca, out int fcValue))
            {
                sumatoria +=
                    (fcValue < 60) ? 3 :
                    (fcValue >= 60 && fcValue <= 110) ? 0 :
                    (fcValue >= 111 && fcValue <= 149) ? 2 :
                    (fcValue >= 150) ? 3 : 0;
            }

            if (int.TryParse(result.Oxigeno, out int o2Value))
            {
                sumatoria +=
                    (o2Value >= 24 && o2Value <= 39) ? 1 :
                    (o2Value >= 40) ? 3 : 0;
            }

            double tpValue;
            if (result.Temperatura.HasValue)
            {
                tpValue = (double)result.Temperatura;
                sumatoria +=
                    (tpValue < 34.0) ? 3 :
                    (tpValue >= 34.0 && tpValue <= 35.0) ? 1 :
                    (tpValue >= 35.1 && tpValue <= 37.9) ? 0 :
                    (tpValue >= 38.0 && tpValue <= 38.9) ? 1 :
                    (tpValue >= 39) ? 3 : 0;
            }


            if (result.EstadoConciencia.HasValue)
            {
                sumatoria = result.EstadoConciencia == true ? 0 : (result.EstadoConciencia == false ? 3 : sumatoria);

            }

            string mensajeAlerta;
            string color;
            if (sumatoria >= 6)
            {
                color = "Rojo";
                mensajeAlerta = "Monitoreo continuo de signos vitales " +
                    " LLAMADO - Emergente al equipo con competencias para el diagnostico";
            }
            else if (sumatoria >= 4 && sumatoria <= 6)
            {
                color = "Naranja";
                mensajeAlerta = "Minimo cada hora " +
                    " LLAMADO - Urgente al equipo medico a cargo de la paciente y al personal" +
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

        private async Task ValidarEstudioSisMae(int? estudioPaciente)
        {
            var ingreso = await _notaenfermeriadbcontext.SisMaes
                .Where(md => md.ConEstudio == estudioPaciente)
                .Select(md => md.ConEstudio)
                .FirstOrDefaultAsync();

            if (ingreso.ToString() == null)
            {
                throw new ValidationEstudioException("El Estudio no existe en la base de datos! ");

            }

        }

        private async Task<NotaEnfermeriaDTOs> ValidarEstudioSignosVitales(int estudioPaciente)
        {
            var ingresoValido = await _notaenfermeriadbcontext.NotasEnfermeria
                 .Where(x => x.Ingreso == estudioPaciente)
                 .OrderByDescending(x => x.Fecha)
                 .Select(x => new NotaEnfermeriaDTOs
                 {
                     Tamizaje = x.Ta,
                     FrecuenciaCardiaca = x.Fc,
                     FrecuenciaRespiratoria = x.Fr,
                     Temperatura = x.Tp,
                     Oxigeno = x.O2,
                     EstadoConciencia = x.EstadoConciencia
                 })
                 .FirstOrDefaultAsync();

            if (ingresoValido == null)
            {
                throw new ValidationEstudioException("El Estudio no existe en la base de datos!");
            }


            return ingresoValido;
        }

   
    }
}
