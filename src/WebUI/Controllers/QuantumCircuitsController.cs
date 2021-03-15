using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuRest.Application.Interfaces;
using QuRest.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
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

        /// <summary>
        /// List all available quantum circuits.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /quantum-circuits
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <returns>A list of all quantum circuits</returns>
        /// <response code="200">Returns the list of all quantum circuits</response>
        [HttpGet]
        [Route("quantum-circuits")]
        [ProducesResponseType(typeof(IEnumerable<QuantumCircuit>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<QuantumCircuit>>> GetAllQuantumCircuitsAsync()
        {
            var circuits = await this.database.QuantumCircuits.ReadAllAsync();
            var realCircuits = circuits.Where(a => !a.Name.EndsWith("_compilation"));

            return new JsonResult(realCircuits);
        }

        /// <summary>
        /// List the names and descriptions of all available quantum circuits.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /quantum-circuits/overview
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <returns>A list of the names and descriptions of all quantum circuits</returns>
        /// <response code="200">Returns the list of the names and descriptions of all quantum circuits</response>
        [HttpGet]
        [Route("quantum-circuits/overview")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<string>>> GetQuantumCircuitsOverviewAsync()
        {
            var overviewStrings = (await this.database.QuantumCircuits.ReadAllAsync())
                .Where(a => !a.Name.EndsWith("_compilation"))
                .Select(a => $"{a.Name}: {a.Description}");

            return new JsonResult(overviewStrings);
        }

        /// <summary>
        /// Create a new quantum circuit.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /quantum-circuits
        ///     {
        ///         "name": "MyCircuit",
        ///         "description": "Perform a coin flip simulation.",
        ///         "size": "1",
        ///         "steps": [
        ///             {
        ///                 "index": 0,
        ///                 "type": "Unitarian"
        ///             },
        ///             {
        ///                 "index": 1,
        ///                 "type": "Hermitian"
        ///             }
        ///         ],
        ///         "unitarians": [
        ///             {
        ///                 "qubits": "0",
        ///                 "type": "H",
        ///                 "index": 0
        ///                 }
        ///         ],
        ///         "hermitians": [
        ///             {
        ///                 "qubits": "0",
        ///                 "type": "X",
        ///                 "index": 1
        ///             }
        ///         ],
        ///     }
        ///
        /// </remarks>
        /// <response code="201">The quantum circuit was created successfully</response>
        /// <response code="409">A quantum circuit with the given name exists already</response>
        [HttpPost]
        [Route("quantum-circuits")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> CreateQuantumCircuitAsync(
            [Required, FromBody] QuantumCircuit quantumCircuit)
        {
            var circuits = await this.database.QuantumCircuits.ReadAllAsync();
            if (circuits.Any(c => c.Name == quantumCircuit.Name))
            {
                return this.Conflict($"A quantum circuit with the name {quantumCircuit.Name} exists already.");
            }

            await this.database.QuantumCircuits.CreateAsync(quantumCircuit);

            return this.NoContent();
        }

        /// <summary>
        /// Get the quantum circuit with the given name.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /quantum-circuits/CoinFlip
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <response code="200">The quantum circuit with the given name</response>
        /// <response code="404">A quantum circuit with the given name cannot be found</response>
        [HttpGet]
        [Route("quantum-circuits/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<QuantumCircuit>> GetQuantumCircuitAsync(
            [Required, FromRoute] string name)
        {
            try
            {
                var circuit = await this.database.QuantumCircuits.ReadAsync(name);

                return new JsonResult(circuit);
            }
            catch
            {
                return this.NotFound($"A quantum circuit with the name {name} cannot be found");
            }
        }

        /// <summary>
        /// Update the quantum circuit with the given name.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /quantum-circuits/CoinFlip
        ///     {
        ///         "name": "CoinFlip",
        ///         "description": "Perform a coin flip simulation.",
        ///         "size": "1",
        ///         "steps": [
        ///             {
        ///                 "index": 0,
        ///                 "type": "Unitarian"
        ///             },
        ///             {
        ///                 "index": 1,
        ///                 "type": "Hermitian"
        ///             }
        ///         ],
        ///         "unitarians": [
        ///             {
        ///                 "qubits": "0",
        ///                 "type": "H",
        ///                 "index": 0
        ///                 }
        ///         ],
        ///         "hermitians": [
        ///             {
        ///                 "qubits": "0",
        ///                 "type": "X",
        ///                 "index": 1
        ///             }
        ///         ],
        ///     }
        ///
        /// </remarks>
        /// <response code="200">The quantum circuit with the given name has been created</response>
        /// <response code="204">The quantum circuit with the given name has been updated</response>
        /// <response code="404">A quantum circuit with the given name cannot be found</response>
        [HttpPut]
        [Route("quantum-circuits/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> UpdateQuantumCircuitAsync(
            [Required, FromRoute] string name,
            [Required, FromBody] QuantumCircuit quantumCircuit)
        {
            try
            {
                await this.database.QuantumCircuits.UpdateAsync(quantumCircuit);

                return this.NoContent();

            }
            catch
            {
                return this.NotFound($"A quantum circuit with the name {name} cannot be found");
            }
        }

        /// <summary>
        /// Delete the quantum circuit with the given name.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /quantum-circuits/CoinFlip
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <response code="204">The quantum circuit with the given name has been deleted</response>
        /// <response code="404">A quantum circuit with the given name cannot be found</response>
        [HttpDelete]
        [Route("quantum-circuits/{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> DeleteQuantumCircuitAsync(
            [Required, FromRoute] string name)
        {
            try
            {
                await this.database.QuantumCircuits.DeleteAsync(name);

                return this.NoContent();

            }
            catch
            {
                return this.NotFound($"A quantum circuit with the name {name} cannot be found");
            }
        }
    }
}
