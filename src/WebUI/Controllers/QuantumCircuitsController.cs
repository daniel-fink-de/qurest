using Microsoft.AspNetCore.Mvc;
using QuRest.Application.Interfaces;
using QuRest.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuRest.WebUI.Controllers
{
    [ApiController, Route("api/")]
    public class AlgorithmsController : ControllerBase
    {
        private readonly IApplicationDbContext database;

        public AlgorithmsController(
            IApplicationDbContext database)
        {
            this.database = database;
        }

        [HttpGet, Route("algorithms")]
        public async Task<ActionResult<IEnumerable<QuantumCircuit>>> GetAllAlgorithmsAsync()
        {
            var algorithms = await this.database.Algorithms.ReadAllAsync();
            var realAlgorithms = algorithms.Where(a => !a.Name.EndsWith("_compilation"));

            return new JsonResult(realAlgorithms);
        }

        [HttpGet, Route("algorithms/overview")]
        public async Task<ActionResult<IEnumerable<string>>> GetAlgorithmsOverviewAsync()
        {
            var overviewStrings = (await this.database.Algorithms.ReadAllAsync())
                .Where(a => !a.Name.EndsWith("_compilation"))
                .Select(a => $"{a.Name}: {a.Description}");

            return new JsonResult(overviewStrings);
        }

        [HttpPost, Route("algorithms")]
        public async Task<ActionResult> CreateAlgorithm(
            [Required, FromBody] QuantumCircuit algorithm)
        {
            await this.database.Algorithms.CreateAsync(algorithm);

            return this.Ok();
        }

        [HttpGet, Route("algorithms/{name}")]
        public async Task<ActionResult<QuantumCircuit>> GetAlgorithmAsync(
            [Required, FromRoute] string name)
        {
            var algorithm = await this.database.Algorithms.ReadAsync(name);

            return new JsonResult(algorithm);
        }

        [HttpPut, Route("algorithms/{name}")]
        public async Task<ActionResult> UpdateAlgorithmAsync(
            [Required, FromRoute] string name,
            [Required, FromBody] QuantumCircuit algorithm)
        {
            await this.database.Algorithms.UpdateAsync(algorithm);

            return this.Ok();
        }

        [HttpDelete, Route("algorithms/{name}")]
        public async Task<ActionResult> DeleteAlgorithmAsync(
            [Required, FromRoute] string name)
        {
            await this.database.Algorithms.DeleteAsync(name);

            return this.Ok();
        }
    }
}
