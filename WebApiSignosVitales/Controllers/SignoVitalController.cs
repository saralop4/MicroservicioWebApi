using DataSignosVitales.Data;
using DataSignosVitales.DTOs;
using DataSignosVitales.Interfaces;
using LogicaSignosVitales.Exceptions;
using LogicaSignosVitales.Interfaces;
using LogicaSignosVitales.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace WebApiSignosVitales.Controllers
{

    [Route("api/v1.0/[controller]")]
    [ApiController]
    [Authorize]
    public class SignoVitalController : ControllerBase
    {
        private readonly INotaEnfermeriaService _notaEnfermeriaService;
        public SignoVitalController(INotaEnfermeriaService notaEnfermeriaService)
        {
            _notaEnfermeriaService = notaEnfermeriaService;
        }

        [HttpPost("CrearSignoVital")]
        public async Task<IActionResult> CrearSignoVital(NotaEnfermeriaDTOs notaEnfermeria)
        {

            try
            {
                await _notaEnfermeriaService.CrearSignoVital(notaEnfermeria);

                var mensaje = $"Signos vitales guardados exitosamente.";
                return Ok(new { Message = mensaje, Nota = notaEnfermeria });
            }
            catch (ValidationNumeroSignoVitalDbUpdateException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ValidationEstudioException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (ValidationCodigoEnfermeraExcepcion ex)
            {
                return BadRequest(new { Error = ex.Message });
            }


        }

        [HttpGet("ObtenerSignoVital")]
        public async Task<IActionResult> ObtenerSignoVital(string? numero_Signovital, int? estudio)
        {

            try
            {

                var result = await _notaEnfermeriaService.ObtenerSignoVital(numero_Signovital, estudio);

                return Ok(result);

            }

            catch (ValidationNumeroSignoVitalRequeridoException)
            {
                return BadRequest("El numero_Signovital es obligatorio.");
            }

        }

        [HttpPut("ActualizarSignoVital")]
        public async Task<IActionResult> ActualizarSignoVital(string numero_Signovital, NotaEnfermeriaDTOs notaEnfermeria)
        {
            try
            {
                await _notaEnfermeriaService.ActualizarSignoVital(numero_Signovital, notaEnfermeria);

                var mensaje = $"Signos vitales se ha actualizado exitosamente.";
                return Ok(new { Message = mensaje, Nota = notaEnfermeria });
            }
            catch (ValidationNumeroSignoVitalRequeridoException)
            {
                return BadRequest("El numero_Signovital es obligatorio.");
            }
            catch (ValidationNumeroSignoVitalDbUpdateException)
            {
                return BadRequest("El registro a actualizar no existe en la base de datos");

            }


        }

    }
}
