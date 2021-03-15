using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using QuRest.Application.Abstractions;
using QuRest.Application.Interfaces;
using QuRest.Domain;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuRest.WebUI.Controllers
{
    [ApiController, Route("api/")]
    public class CompilationsController : ControllerBase
    {
        private readonly IApplicationDbContext database;

        public CompilationsController(
            IApplicationDbContext database)
        {
            this.database = database;
        }

        /// <summary>
        /// List all available compilations.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /compilations
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <returns>A list of all compilations</returns>
        /// <response code="200">Returns the list of all compilations</response>
        [HttpGet]
        [Route("compilations")]
        [ProducesResponseType(typeof(IEnumerable<QuantumCircuit>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<QuantumCircuit>>> GetAllCompilationsAsync()
        {
            var circuits = await this.database.QuantumCircuits.ReadAllAsync();
            var compilations = circuits.Where(a => a.Name.EndsWith("_compilation"));

            return new JsonResult(compilations);
        }

        /// <summary>
        /// Get the compilation with the given name.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /compilations/CoinFlip
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <returns>The compilation with the given name</returns>
        /// <response code="200">Returns the compilation with the given name</response>
        /// <response code="404">A compilation with the given name could not be found</response>
        [HttpGet]
        [Route("compilations/{name}")]
        [ProducesResponseType(typeof(QuantumCircuit), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<QuantumCircuit>> GetCompilationAsync(
            [Required, FromRoute] string name)
        {
            try
            {
                var compilation = await this.database.QuantumCircuits.ReadAsync($"{name}_compilation");

                return new JsonResult(compilation);
            }
            catch
            {
                return this.NotFound($"A compilation with the name {name} could not be found");
            }
        }

        /// <summary>
        /// Compiles the quantum circuit with the given name and placeholder and parameter mapping.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /compilations/CoinFlip
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <response code="204">The quantum circuit has been compiled successfully</response>
        /// <response code="404">A compilation with the given name could not be found</response>
        /// <response code="400">An error occurred during compilation</response>
        [HttpPut]
        [Route("compilations/{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CompileAsync(
            [Required, FromRoute] string name,
            [FromServices] IQxmlCompiler compiler,
            [FromQuery] IDictionary<string, double>? parameterMapping = null,
            [FromQuery] IDictionary<string, string>? placeholderMapping = null)
        {
            try
            {
                var circuits = (await this.database.QuantumCircuits.ReadAllAsync()).ToList();
                var circuit = circuits.FirstOrDefault(a => a.Name == name);

                if (circuit is null)
                {
                    return this.NotFound($"A quantum circuit with the name {name} could not be found");
                }

                Dictionary<string, QuantumCircuit> placeholders = new();

                if (placeholderMapping is not null)
                {
                    placeholders = circuits.Where(a => placeholderMapping.ContainsKey(a.Name))
                        .ToDictionary(a => a.Name, a => a);
                }

                compiler.WithParameterMapping(parameterMapping ?? new Dictionary<string, double>());
                compiler.WithPlaceholderMapping(placeholders);

                var compilation = await compiler.CompileAsync(circuit);
                await this.database.QuantumCircuits.CreateAsync(compilation);

                return this.NoContent();
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Export the compilation with the given name to qasm.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /compilations/CoinFlip/qasm
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <response code="200">The qasm string of the compilation</response>
        /// <response code="404">A compilation with the given name could not be found</response>
        /// <response code="400">An error occurred during exporting to qasm</response>
        [HttpGet]
        [Route("compilations/{name}/qasm")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces(MediaTypeNames.Text.Plain)]
        public async Task<ActionResult<QuantumCircuit>> GetQasmAsync(
            [FromServices] IQxmlTranslator translator,
            [Required, FromRoute] string name)
        {
            try
            {
                var allCompilations = (await this.database.QuantumCircuits.ReadAllAsync()).ToList();

                if (allCompilations.All(c => c.Name != $"{name}_compilation"))
                {
                    return this.NotFound($"A compilation with the name {name} could not be found");
                }

                var compilation = allCompilations.First(c => c.Name == $"{name}_compilation");
                var qasm = await translator.TranslateToQasmAsync(compilation);

                return new ContentResult
                {
                    Content = qasm,
                    ContentType = "text/plain",
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch
            {
                return this.BadRequest($"An error occurred while exporting to qasm");
            }
        }

        /// <summary>
        /// Delete the compilation with the given name.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /compilations/CoinFlip
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <response code="204">The compilation has been deleted successfully</response>
        /// <response code="404">A compilation with the given name could not be found</response>
        [HttpDelete]
        [Route("compilations/{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteCompilationAsync(
            [Required, FromRoute] string name)
        {
            try
            {
                await this.database.QuantumCircuits.DeleteAsync($"{name}_compilation");

                return this.NoContent();
            }
            catch
            {
                return this.NotFound($"A compilation with the name {name} could not be found");
            }
        }

        /// <summary>
        /// Export the compilation with the given name to svg.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /compilations/CoinFlip/svg
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <response code="204">The svg of the compilation</response>
        /// <response code="404">A compilation with the given name could not be found</response>
        /// <response code="400">An error occurred during exporting to svg</response>
        [FeatureGate(FeatureFlags.SvgExport)]
        [HttpGet]
        [Route("compilations/{name}/svg")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Text.Html)]
        public async Task<ActionResult<string>> GetSvgAsync(
            [FromServices] IQxmlDrawer drawer,
            [Required, FromRoute] string name)
        {
            try
            {
                var allCompilations = (await this.database.QuantumCircuits.ReadAllAsync()).ToList();

                if (allCompilations.All(c => c.Name != $"{name}_compilation"))
                {
                    return this.NotFound($"A compilation with the name {name} could not be found");
                }

                var compilation = allCompilations.First(c => c.Name == $"{name}_compilation");
                var svg = await drawer.DrawAsync(compilation);

                return new ContentResult
                {
                    Content = svg,
                    ContentType = "text/html",
                    StatusCode = (int) HttpStatusCode.OK
                };
            }
            catch
            {
                return this.BadRequest("An error occurred during exporting to svg");
            }
        }

        /// <summary>
        /// Export the compilation with the given name to png.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /compilations/CoinFlip/png
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <response code="204">The png of the compilation</response>
        /// <response code="404">A compilation with the given name could not be found</response>
        /// <response code="400">An error occurred during exporting to png</response>
        [FeatureGate(FeatureFlags.PngExport)]
        [HttpGet]
        [Route("compilations/{name}/png")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("image/png")]
        public async Task<ActionResult<string>> GetPngAsync(
            [FromServices] IQxmlDrawer drawer,
            [Required, FromRoute] string name)
        {
            try
            {
                var allCompilations = (await this.database.QuantumCircuits.ReadAllAsync()).ToList();

                if (allCompilations.All(c => c.Name != $"{name}_compilation"))
                {
                    return this.NotFound($"A compilation with the name {name} could not be found");
                }

                var compilation = allCompilations.First(c => c.Name == $"{name}_compilation");
                var svg = await drawer.DrawAsync(compilation);

                await System.IO.File.WriteAllTextAsync($"{name}.svg", svg);

                var svgStream = new MemoryStream();
                var writer = new StreamWriter(svgStream);
                await writer.WriteAsync(svg);
                await writer.FlushAsync();
                svgStream.Position = 0;

                var file = new FileInfo($"{name}.png");
                var svgObj = new SkiaSharp.Extended.Svg.SKSvg();
                var picture = svgObj.Load(svgStream);
                var size = new SKSizeI(
                    (int)Math.Ceiling(picture.CullRect.Width) * 2,
                    (int)Math.Ceiling(picture.CullRect.Height) * 2
                );
                var matrix = SKMatrix.MakeScale(1, 1);
                var img = SKImage.FromPicture(picture, size, matrix);

                var data = img.Encode(SKEncodedImageFormat.Png, 100);
                await using (var fileStream = file.OpenWrite())
                {
                    data.SaveTo(fileStream);
                }

                return this.PhysicalFile(file.FullName, "image/png");
            }
            catch
            {
                return this.BadRequest("An error occurred during exporting to png");
            }
        }
    }
}
