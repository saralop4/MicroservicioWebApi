using DataSignosVitales.DTOs;
using DataSignosVitales.Entities.NotaEnfermeriaModels;
using DataSignosVitales.Interfaces;
using LogicaSignosVitales.Exceptions;
using LogicaSignosVitales.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;


namespace LogicaSignosVitales.Services

{
    public class NotaEnfemeriaService : INotaEnfermeriaService
    {
        private readonly INotaEnfermeriaDbContext _notaenfermeriadbcontext;

        public NotaEnfemeriaService(INotaEnfermeriaDbContext notaenfermeriadbcontext)
        {
            _notaenfermeriadbcontext = notaenfermeriadbcontext;
        }
        public async Task ActualizarSignoVital(string? numero_SignoVital, NotaEnfermeriaDTOs? notaEnfermeriaDtos)
        {
           if(numero_SignoVital != null && notaEnfermeriaDtos != null)
            {
                var entidadExistente = await ValidarUpdateNumero(numero_SignoVital);


                entidadExistente.Fecha = notaEnfermeriaDtos.Fecha;
                entidadExistente.Hora = notaEnfermeriaDtos.Hora;
                entidadExistente.Resumen = notaEnfermeriaDtos.Nota;
                entidadExistente.Enfermera = notaEnfermeriaDtos.Enfermera;
                entidadExistente.CodEnfer = notaEnfermeriaDtos.CodigoEnfermera;
                entidadExistente.Ta = notaEnfermeriaDtos.TensionArterial;
                entidadExistente.Fc = notaEnfermeriaDtos.FrecuenciaCardiaca;
                entidadExistente.Fr = notaEnfermeriaDtos.FrecuenciaRespiratoria;
                entidadExistente.Ps = notaEnfermeriaDtos.Peso;
                entidadExistente.Tp = notaEnfermeriaDtos.Temperatura;
                entidadExistente.O2 = notaEnfermeriaDtos.Oxigeno;
                entidadExistente.Glucometria = notaEnfermeriaDtos.Glucometria;
                entidadExistente.Ufuncional = notaEnfermeriaDtos.UnidadFuncional;
                entidadExistente.Tam = notaEnfermeriaDtos.Tamizaje;
                entidadExistente.EstadoConciencia = notaEnfermeriaDtos.EstadoConciencia;


                await _notaenfermeriadbcontext.SaveChangesAsync();
            }
            throw new ValidationNumeroSignoVitalRequeridoException();
                  
        }
    
        public async Task<NotaEnfermeriaDTOs> CrearSignoVital(NotaEnfermeriaDTOs notaEnfermeriaDtos)
        {
           
            await ValidarNumero(notaEnfermeriaDtos.Numero_SignoVital);

            await ValidarEstudioSisMae(notaEnfermeriaDtos.Estudio);

            await ValidarCodigoEnfermera(notaEnfermeriaDtos.CodigoEnfermera);

                var create = new NotasEnfermerium
                {

                    Numero = notaEnfermeriaDtos.Numero_SignoVital,
                    Ingreso = notaEnfermeriaDtos.Estudio,
                    Fecha = notaEnfermeriaDtos.Fecha,
                    Hora = notaEnfermeriaDtos.Hora,
                    Resumen = notaEnfermeriaDtos.Nota,
                    Enfermera = notaEnfermeriaDtos.Enfermera,
                    CodEnfer = notaEnfermeriaDtos.CodigoEnfermera,
                    Ta = notaEnfermeriaDtos.TensionArterial,
                    Fc = notaEnfermeriaDtos.FrecuenciaCardiaca,
                    Fr = notaEnfermeriaDtos.FrecuenciaRespiratoria,
                    Ps = notaEnfermeriaDtos.Peso,
                    Tp = notaEnfermeriaDtos.Temperatura,
                    O2 = notaEnfermeriaDtos.Oxigeno,
                    Glucometria = notaEnfermeriaDtos.Glucometria,
                    Ufuncional = notaEnfermeriaDtos.UnidadFuncional,
                    Tam = notaEnfermeriaDtos.Tamizaje,
                    EstadoConciencia = notaEnfermeriaDtos.EstadoConciencia

                };

                
                _notaenfermeriadbcontext.NotasEnfermeria.Add(create);
                await _notaenfermeriadbcontext.SaveChangesAsync();
                return notaEnfermeriaDtos;
            
        }

        public async Task<NotaEnfermeriaDTOs> ObtenerSignoVital(string? numero_SignoVital, int? estudio)

        {

            if( numero_SignoVital != null)
            {
                var query = _notaenfermeriadbcontext.NotasEnfermeria.AsQueryable();

                var registroNotaEnfermeria = await query
                    .Where(x => (x.Numero == numero_SignoVital) && (!estudio.HasValue || x.Ingreso == estudio))
                    .FirstOrDefaultAsync();

                var numeroDtos = new NotaEnfermeriaDTOs
                {
                    Numero_SignoVital = registroNotaEnfermeria.Numero,
                    Estudio = (int)registroNotaEnfermeria.Ingreso,
                    Fecha = registroNotaEnfermeria.Fecha,
                    Hora = registroNotaEnfermeria.Hora,
                    Nota = registroNotaEnfermeria.Resumen,
                    Enfermera = registroNotaEnfermeria.Enfermera,
                    CodigoEnfermera = registroNotaEnfermeria.CodEnfer,
                    TensionArterial = registroNotaEnfermeria.Ta,
                    FrecuenciaCardiaca = registroNotaEnfermeria.Fc,
                    FrecuenciaRespiratoria = registroNotaEnfermeria.Fr,
                    Peso = registroNotaEnfermeria.Ps,
                    Temperatura = registroNotaEnfermeria.Tp,
                    Oxigeno = registroNotaEnfermeria.O2,
                    Glucometria = registroNotaEnfermeria.Glucometria,
                    UnidadFuncional = registroNotaEnfermeria.Ufuncional,
                    Tamizaje = registroNotaEnfermeria.Tam,
                    EstadoConciencia = registroNotaEnfermeria.EstadoConciencia
                };

                return numeroDtos;

            }

            throw new ValidationNumeroSignoVitalRequeridoException();

           
            }

        private async Task ValidarEstudioSisMae(int estudio)
        {
            var ingreso = await _notaenfermeriadbcontext.SisMaes
                .Where(md => md.ConEstudio == estudio)
                .Select(md => md.ConEstudio)
                .FirstOrDefaultAsync();

            if (ingreso.ToString() == null)
            {
                throw new ValidationEstudioException("El Estudio no existe en la base de datos! ");

            }

        }

        private async Task ValidarNumero(string numero_SignoVital)
        {
            var numeroSignoVital = await _notaenfermeriadbcontext.NotasEnfermeria
                .Where(nm => nm.Numero == numero_SignoVital)
                .Select(nm => nm.Numero)
                .FirstOrDefaultAsync();

            if (numeroSignoVital != null)
            {
                throw new ValidationNumeroSignoVitalDbUpdateException("El numero del signo vital ya existe");
            }
           
        }

        private async Task<int> ValidarCodigoEnfermera(string? codigoEnfermera)
        {
            

            if (codigoEnfermera != null)
            {
                var enfermera = await _notaenfermeriadbcontext.SisMedis
                 .Where(sm => sm.Codigo.ToString() == codigoEnfermera)
                 .Select(sm => sm.Codigo)
                 .FirstOrDefaultAsync();

                return enfermera;

            }
            throw new ValidationCodigoEnfermeraExcepcion("El codigo de la enfermera no existe");
        }

        private async Task<NotasEnfermerium?> ValidarUpdateNumero(string? numero_SignoVital)
        {
            if(numero_SignoVital != null)
            {
                var entidadExistente = await _notaenfermeriadbcontext.NotasEnfermeria
               .Where(nm => nm.Numero == numero_SignoVital)
               .FirstOrDefaultAsync();

                if(entidadExistente != null)
                {
                    return entidadExistente;
                }
                throw new ValidationNumeroSignoVitalDbUpdateException();
            }
            throw new ValidationNumeroSignoVitalRequeridoException();
            
        }



    }
}
