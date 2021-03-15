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
using System.Threading.Tasks;

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

        [HttpGet, Route("compilations")]
        public async Task<ActionResult> GetAllCompilationsAsync()
        {
            var algorithms = await this.database.Algorithms.ReadAllAsync();
            var compilations = algorithms.Where(a => a.Name.EndsWith("_compilation"));

            return new JsonResult(compilations);
        }

        [HttpGet, Route("compilations/{name}")]
        public async Task<ActionResult> GetCompilationAsync(
            [Required, FromRoute] string name)
        {
            var compilation = await this.database.Algorithms.ReadAsync($"{name}_compilation");

            return new JsonResult(compilation);
        }

        [HttpPut, Route("compilations/{name}")]
        public async Task<ActionResult> CompileAlgorithmAsync(
            [Required, FromRoute] string name,
            [FromServices] IQxmlCompiler compiler,
            [FromQuery] IDictionary<string, double>? parameterMapping = null,
            [FromQuery] IDictionary<string, string>? placeholderMapping = null)
        {
            var algorithms = (await this.database.Algorithms.ReadAllAsync()).ToList();
            var algorithm = algorithms.First(a => a.Name == name);

            Dictionary<string, QuantumCircuit> placeholders = new();

            if (placeholderMapping is not null)
            {
                placeholders = algorithms.Where(a => placeholderMapping.ContainsKey(a.Name))
                    .ToDictionary(a => a.Name, a => a);
            }

            compiler.WithParameterMapping(parameterMapping ?? new Dictionary<string, double>());
            compiler.WithPlaceholderMapping(placeholders);

            var compilation = await compiler.CompileAsync(algorithm);
            await this.database.Algorithms.CreateAsync(compilation);

            return this.Ok();
        }

        [HttpGet, Route("compilations/{name}/qasm")]
        public async Task<ActionResult<QuantumCircuit>> GetQasmAsync(
            [FromServices] IQxmlTranslator translator,
            [Required, FromRoute] string name)
        {
            var compilation = await this.database.Algorithms.ReadAsync($"{name}_compilation");
            var qasm = await translator.TranslateToQasmAsync(compilation);

            return new ContentResult
            {
                Content = qasm,
                ContentType = "text/plain",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        [HttpDelete, Route("compilations/{name}")]
        public async Task<ActionResult> DeleteCompilationAsync(
            [Required, FromRoute] string name)
        {
            await this.database.Algorithms.DeleteAsync($"{name}_compilation");

            return this.Ok();
        }

        [FeatureGate(FeatureFlags.SvgExport), HttpGet, Route("compilations/{name}/svg")]
        public async Task<ActionResult<string>> GetSvgAsync(
            [FromServices] IQxmlDrawer drawer,
            [Required, FromRoute] string name)
        {
            var compilation = await this.database.Algorithms.ReadAsync($"{name}_compilation");
            var svg = await drawer.DrawAsync(compilation);

            return new ContentResult
            {
                Content = svg,
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        [FeatureGate(FeatureFlags.PngExport), HttpGet, Route("compilations/{name}/png")]
        public async Task<ActionResult<string>> GetPngAsync(
            [FromServices] IQxmlDrawer drawer,
            [Required, FromRoute] string name)
        {
            var compilation = await this.database.Algorithms.ReadAsync($"{name}_compilation");
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
    }
}
