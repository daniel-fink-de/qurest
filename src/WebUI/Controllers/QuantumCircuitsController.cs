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
    public class QuantumCircuitsController : ControllerBase
    {
        private readonly IApplicationDbContext database;

        public QuantumCircuitsController(
            IApplicationDbContext database)
        {
            this.database = database;
        }

        [HttpGet, Route("quantum-circuits")]
        public async Task<ActionResult<IEnumerable<QuantumCircuit>>> GetAllQuantumCircuitsAsync()
        {
            var circuits = await this.database.QuantumCircuits.ReadAllAsync();
            var realCircuits = circuits.Where(a => !a.Name.EndsWith("_compilation"));

            return new JsonResult(realCircuits);
        }

        [HttpGet, Route("quantum-circuits/overview")]
        public async Task<ActionResult<IEnumerable<string>>> GetQuantumCircuitsOverviewAsync()
        {
            var overviewStrings = (await this.database.QuantumCircuits.ReadAllAsync())
                .Where(a => !a.Name.EndsWith("_compilation"))
                .Select(a => $"{a.Name}: {a.Description}");

            return new JsonResult(overviewStrings);
        }

        [HttpPost, Route("quantum-circuits")]
        public async Task<ActionResult> CreateQuantumCircuitAsync(
            [Required, FromBody] QuantumCircuit quantumCircuit)
        {
            await this.database.QuantumCircuits.CreateAsync(quantumCircuit);

            return this.Ok();
        }

        [HttpGet, Route("quantum-circuits/{name}")]
        public async Task<ActionResult<QuantumCircuit>> GetQuantumCircuitAsync(
            [Required, FromRoute] string name)
        {
            var circuit = await this.database.QuantumCircuits.ReadAsync(name);

            return new JsonResult(circuit);
        }

        [HttpPut, Route("quantum-circuits/{name}")]
        public async Task<ActionResult> UpdateQuantumCircuitAsync(
            [Required, FromRoute] string name,
            [Required, FromBody] QuantumCircuit quantumCircuit)
        {
            await this.database.QuantumCircuits.UpdateAsync(quantumCircuit);

            return this.Ok();
        }

        [HttpDelete, Route("quantum-circuits/{name}")]
        public async Task<ActionResult> DeleteQuantumCircuitAsync(
            [Required, FromRoute] string name)
        {
            await this.database.QuantumCircuits.DeleteAsync(name);

            return this.Ok();
        }
    }
}
