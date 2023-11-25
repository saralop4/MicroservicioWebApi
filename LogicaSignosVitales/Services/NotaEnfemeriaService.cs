using DataSignosVitales.DTOs;
using DataSignosVitales.Entities.NotaEnfermeriaModels;
using DataSignosVitales.Interfaces;
using LogicaSignosVitales.Exceptions;
using LogicaSignosVitales.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace LogicaSignosVitales.Services

{
    public class NotaEnfemeriaService : INotaEnfermeriaService
    {
        private readonly INotaEnfermeriaDbContext _notaenfermeriadbcontext;

        public NotaEnfemeriaService(INotaEnfermeriaDbContext notaenfermeriadbcontext)
        {
            _notaenfermeriadbcontext = notaenfermeriadbcontext;
        }
        public async Task ActualizarPorNumeroSignovital(string? numero_SignoVital, NotaEnfermeriaDTOs? notaEnfermeriaDtos)
        {         

            if (numero_SignoVital != null && notaEnfermeriaDtos != null)
            {
                var entidadExistente = await ValidarUpdatePorNumero(numero_SignoVital);

                if (entidadExistente != null)
                {
                 
                    entidadExistente.Ingreso = notaEnfermeriaDtos.Estudio;
                    entidadExistente.Fecha = notaEnfermeriaDtos.Fecha;
                    entidadExistente.Hora = notaEnfermeriaDtos.Hora;
                    entidadExistente.Resumen = notaEnfermeriaDtos.Nota ?? " ";
                    entidadExistente.Enfermera = notaEnfermeriaDtos.Enfermera;
                    entidadExistente.CodEnfer = notaEnfermeriaDtos.CodigoEnfermera;
                    entidadExistente.Ta = notaEnfermeriaDtos.TensionArterial;
                    entidadExistente.Fc = notaEnfermeriaDtos.FrecuenciaCardiaca;
                    entidadExistente.Fr = notaEnfermeriaDtos.FrecuenciaRespiratoria;
                    entidadExistente.Ps = notaEnfermeriaDtos.Peso;
                    entidadExistente.Tp = notaEnfermeriaDtos.Temperatura;
                    entidadExistente.O2 = notaEnfermeriaDtos.Oxigeno;
                    entidadExistente.SoloVitales = notaEnfermeriaDtos.SoloVitales;
                    entidadExistente.Cerrada = notaEnfermeriaDtos.Cerrada;
                    entidadExistente.Glucometria = notaEnfermeriaDtos.Glucometria;
                    entidadExistente.Ufuncional = notaEnfermeriaDtos.UnidadFuncional;
                    entidadExistente.Tam = notaEnfermeriaDtos.Tamizaje;
                    entidadExistente.EstadoConciencia = notaEnfermeriaDtos.EstadoConciencia;
                    entidadExistente.NroEvolucionRelacionado = notaEnfermeriaDtos.NroEvolucionRelacionado ?? null;


                    await _notaenfermeriadbcontext.SaveChangesAsync();
                }
                else
                {
                    throw new ValidationEntityExisting();
                }

               
            }
            else
            {
                throw new ValidationArgumentsEntityNullException();
            }
        }

        public async Task<NotaEnfermeriaDTOs> CrearSignoVital(NotaEnfermeriaDTOs notaEnfermeriaDtos)
        {
    
            await ValidarNumero(notaEnfermeriaDtos.Numero_SignoVital);

            var estudioValido =  await ValidarEstudioSisMae(notaEnfermeriaDtos.Estudio);

            await ValidarCodigoEnfermera(notaEnfermeriaDtos.CodigoEnfermera);

            var create = new NotasEnfermerium
            {
                Numero = notaEnfermeriaDtos.Numero_SignoVital,
                Ingreso = estudioValido,
                Fecha = notaEnfermeriaDtos.Fecha,
                Hora = notaEnfermeriaDtos.Hora,
                Resumen = notaEnfermeriaDtos.Nota ?? " ",
                Enfermera = notaEnfermeriaDtos.Enfermera,
                CodEnfer = notaEnfermeriaDtos.CodigoEnfermera,
                Ta = notaEnfermeriaDtos.TensionArterial,
                Fc = notaEnfermeriaDtos.FrecuenciaCardiaca,
                Fr = notaEnfermeriaDtos.FrecuenciaRespiratoria,
                Ps = notaEnfermeriaDtos.Peso,
                Tp = notaEnfermeriaDtos.Temperatura,
                O2 = notaEnfermeriaDtos.Oxigeno,
                SoloVitales = notaEnfermeriaDtos.SoloVitales,
                Cerrada = notaEnfermeriaDtos.Cerrada,
                Glucometria = notaEnfermeriaDtos.Glucometria,
                Ufuncional = notaEnfermeriaDtos.UnidadFuncional,
                Tam = notaEnfermeriaDtos.Tamizaje,
                EstadoConciencia = notaEnfermeriaDtos.EstadoConciencia,
                NroEvolucionRelacionado = notaEnfermeriaDtos.NroEvolucionRelacionado

            };


            _notaenfermeriadbcontext.NotasEnfermeria.Add(create);
            await _notaenfermeriadbcontext.SaveChangesAsync();
            Console.WriteLine(notaEnfermeriaDtos);
            return notaEnfermeriaDtos;

        }

        public async Task<NotaEnfermeriaDTOs> ObtenerSignoVital(string? numero_SignoVital)
        {
            if (string.IsNullOrEmpty(numero_SignoVital))
            {
                throw new ValidationNumeroSignoVitalRequeridoException();
            }

            var registroNotaEnfermeria = await _notaenfermeriadbcontext.NotasEnfermeria
                .FirstOrDefaultAsync(u => u.Numero == numero_SignoVital);

            if (registroNotaEnfermeria != null)
            {
                var numeroDtos = new NotaEnfermeriaDTOs
                {
                    Numero_SignoVital = registroNotaEnfermeria.Numero,
                    Estudio = registroNotaEnfermeria.Ingreso.HasValue ? (int)registroNotaEnfermeria.Ingreso : 0,
                    Fecha = registroNotaEnfermeria.Fecha ?? DateTime.MinValue,
                    Hora = registroNotaEnfermeria.Hora,
                    Nota = registroNotaEnfermeria.Resumen ?? string.Empty,
                    Enfermera = registroNotaEnfermeria.Enfermera ?? string.Empty,
                    CodigoEnfermera = registroNotaEnfermeria.CodEnfer ?? "0",
                    TensionArterial = registroNotaEnfermeria.Ta ?? "0",
                    FrecuenciaCardiaca = registroNotaEnfermeria.Fc ?? "0",
                    FrecuenciaRespiratoria = registroNotaEnfermeria.Fr ?? "0",
                    Peso = registroNotaEnfermeria.Ps ?? "0",
                    Temperatura = registroNotaEnfermeria.Tp ?? 0.0m,
                    Oxigeno = registroNotaEnfermeria.O2 ?? "0",
                    SoloVitales = registroNotaEnfermeria.SoloVitales.HasValue ? (int)registroNotaEnfermeria.SoloVitales : 0,
                    Cerrada = registroNotaEnfermeria.Cerrada,
                    Glucometria = registroNotaEnfermeria.Glucometria ?? 0.0m,
                    UnidadFuncional = registroNotaEnfermeria.Ufuncional.HasValue ? (int)registroNotaEnfermeria.Ufuncional : 0,
                    Tamizaje = registroNotaEnfermeria.Tam ?? string.Empty,
                    EstadoConciencia = registroNotaEnfermeria.EstadoConciencia ?? false,
                    NroEvolucionRelacionado = registroNotaEnfermeria.NroEvolucionRelacionado ?? "0"
                };

                return numeroDtos;
            }

            throw new ValidationEntityExisting();
        }
        public async Task<NotaEnfermeriaDTOs> ObtenerSignoVitalxEvolucion(string? nroevolucionrelacionado)
        {
            if (string.IsNullOrEmpty(nroevolucionrelacionado))
            {
                throw new ValidationNroEvolucionSignoVitalRequeridoException();
            }

            var registroNotaEnfermeria = await _notaenfermeriadbcontext.NotasEnfermeria
                .FirstOrDefaultAsync(u => u.NroEvolucionRelacionado == nroevolucionrelacionado);

            if (registroNotaEnfermeria != null)
            {
                var numeroDtos = new NotaEnfermeriaDTOs
                {
                    Numero_SignoVital = registroNotaEnfermeria.Numero,
                    Estudio = registroNotaEnfermeria.Ingreso.HasValue ? (int)registroNotaEnfermeria.Ingreso : 0,
                    Fecha = registroNotaEnfermeria.Fecha ?? DateTime.MinValue,
                    Hora = registroNotaEnfermeria.Hora,
                    Nota = registroNotaEnfermeria.Resumen ?? string.Empty,
                    Enfermera = registroNotaEnfermeria.Enfermera ?? string.Empty,
                    CodigoEnfermera = registroNotaEnfermeria.CodEnfer ?? "0",
                    TensionArterial = registroNotaEnfermeria.Ta ?? "0",
                    FrecuenciaCardiaca = registroNotaEnfermeria.Fc ?? "0",
                    FrecuenciaRespiratoria = registroNotaEnfermeria.Fr ?? "0",
                    Peso = registroNotaEnfermeria.Ps ?? "0",
                    Temperatura = registroNotaEnfermeria.Tp ?? 0.0m,
                    Oxigeno = registroNotaEnfermeria.O2 ?? "0",
                    SoloVitales = registroNotaEnfermeria.SoloVitales.HasValue ? (int)registroNotaEnfermeria.SoloVitales : 0,
                    Cerrada = registroNotaEnfermeria.Cerrada,
                    Glucometria = registroNotaEnfermeria.Glucometria ?? 0.0m,
                    UnidadFuncional = registroNotaEnfermeria.Ufuncional.HasValue ? (int)registroNotaEnfermeria.Ufuncional : 0,
                    Tamizaje = registroNotaEnfermeria.Tam ?? string.Empty,
                    EstadoConciencia = registroNotaEnfermeria.EstadoConciencia ?? false,
                    NroEvolucionRelacionado = registroNotaEnfermeria.NroEvolucionRelacionado ?? "0"
                };

                return numeroDtos;
            }

            throw new ValidationEntityExisting();
        }

        private async Task<int> ValidarEstudioSisMae(int? estudio)
        {
            if (estudio.HasValue)
            {
                var ingreso = await _notaenfermeriadbcontext.SisMaes
                    .Where(md => md.ConEstudio == estudio.Value)
                    .Select(md => md.ConEstudio)
                    .FirstOrDefaultAsync();

                if (ingreso != 0) 
                {
                    return (int)ingreso;
                }
            }

            throw new ValidationEstudioException();
        }

        private async Task ValidarNumero(string numero_SignoVital)
        {
           
                var numeroSignoVital = await _notaenfermeriadbcontext.NotasEnfermeria
                    .Where(nm => nm.Numero == numero_SignoVital)
                    .Select(nm => nm.Numero)
                    .FirstOrDefaultAsync();

                if (numeroSignoVital != null)
                {
                throw new ValidationNumeroSignoVitalDbUpdateException();
                } 

        }

        private async Task ValidarCodigoEnfermera(string codigoEnfermera)
        {
            var enfermera = await _notaenfermeriadbcontext.SisMedis
                .Where(sm => sm.Codigo.ToString() == codigoEnfermera)
                .Select(sm => sm.Codigo)
                .FirstOrDefaultAsync();

            if (enfermera.ToString() == null)
            {
                throw new ValidationCodigoEnfermeraExcepcion();
            }


        }

        private async Task<NotasEnfermerium?> ValidarUpdatePorNumero(string numero_SignoVital)
        {
           
                var objetoExistente = await _notaenfermeriadbcontext.NotasEnfermeria
               .Where(nm => nm.Numero == numero_SignoVital)
               .FirstOrDefaultAsync();

            if (objetoExistente != null)
            {
                return objetoExistente;
            }
            return null;

        }

        private async Task<NotasEnfermerium?> ValidarUpdatePorNroEvolucionRelacionado(string nro_EvolucionRelacionado)
        {
            var objetoExistente = await _notaenfermeriadbcontext.NotasEnfermeria
                .Where(nm => nm.NroEvolucionRelacionado == nro_EvolucionRelacionado)
                .FirstOrDefaultAsync();

            return objetoExistente;
        }

        public async Task ActualizarPorNroEvolucionRelacionado(string? nroEvolucionRelacionado, NotaEnfermeriaDTOs notaEnfermeriaDtos)
        {


            if (nroEvolucionRelacionado != null && notaEnfermeriaDtos != null)
            {

                 var entityExisting = await ValidarUpdatePorNroEvolucionRelacionado(nroEvolucionRelacionado);


                if (entityExisting != null)
                {
            
                    entityExisting.Ingreso = notaEnfermeriaDtos.Estudio;
                    entityExisting.Fecha = notaEnfermeriaDtos.Fecha;
                    entityExisting.Hora = notaEnfermeriaDtos.Hora;
                    entityExisting.Resumen = notaEnfermeriaDtos.Nota;
                    entityExisting.Enfermera = notaEnfermeriaDtos.Enfermera;
                    entityExisting.CodEnfer = notaEnfermeriaDtos.CodigoEnfermera;
                    entityExisting.Ta = notaEnfermeriaDtos.TensionArterial;
                    entityExisting.Fc = notaEnfermeriaDtos.FrecuenciaCardiaca;
                    entityExisting.Fr = notaEnfermeriaDtos.FrecuenciaRespiratoria;
                    entityExisting.Ps = notaEnfermeriaDtos.Peso;
                    entityExisting.Tp = notaEnfermeriaDtos.Temperatura;
                    entityExisting.O2 = notaEnfermeriaDtos.Oxigeno;
                    entityExisting.SoloVitales = notaEnfermeriaDtos.SoloVitales;
                    entityExisting.Cerrada = notaEnfermeriaDtos.Cerrada;
                    entityExisting.Glucometria = notaEnfermeriaDtos.Glucometria;
                    entityExisting.Ufuncional = notaEnfermeriaDtos.UnidadFuncional;
                    entityExisting.Tam = notaEnfermeriaDtos.Tamizaje;
                    entityExisting.EstadoConciencia = notaEnfermeriaDtos.EstadoConciencia;
                    entityExisting.NroEvolucionRelacionado = notaEnfermeriaDtos.NroEvolucionRelacionado ?? null;

                    await _notaenfermeriadbcontext.SaveChangesAsync();
                }
                else
                {
                    throw new ValidationEntityExisting();
                }
            }
            else
            {
                throw new ValidationArgumentsEntityNullException();
            }
        }

    }
}
