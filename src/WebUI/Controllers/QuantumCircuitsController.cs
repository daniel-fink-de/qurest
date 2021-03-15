using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuRest.Application.Interfaces;
using QuRest.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        [ProducesResponseType(typeof(QuantumCircuit), StatusCodes.Status200OK)]
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
        /// Get the quantum circuit with the given name as qxml.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /quantum-circuits/CoinFlip/qxml
        ///     {
        ///
        ///     }
        ///
        /// </remarks>
        /// <response code="200">The <a href="https://github.com/StuttgarterDotNet/qurest/blob/main/src/Domain/qxml.xsd">qxml</a> of the quantum circuit with the given name</response>
        /// <response code="404">A quantum circuit with the given name cannot be found</response>
        [HttpGet]
        [Route("quantum-circuits/{name}/qxml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Produces(MediaTypeNames.Text.Plain)]
        public async Task<ActionResult<string>> GetQuantumCircuitQxmlAsync(
            [Required, FromRoute] string name)
        {
            try
            {
                var circuit = await this.database.QuantumCircuits.ReadAsync(name);

                var serializer = new XmlSerializer(typeof(QuantumCircuit));
                await using var stringWriter = new StringWriter();
                serializer.Serialize(stringWriter, circuit);

                return stringWriter.ToString();
            }
            catch
            {
                return this.NotFound($"A quantum circuit with the name {name} cannot be found");
            }
        }

        /// <summary>
        /// Create a quantum circuit with the given qxml.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /quantum-circuits/MyCircuit/qxml
        ///
        ///     &lt;?xml version="1.0" encoding="utf-16"?&gt;
        ///     &lt;quantumCircuit xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="MyCircuit" size="1"&gt;
        ///     &lt;steps xmlns="qxml"&gt;
        ///     &lt;step index="0" type="Unitarian" /&gt;
        ///     &lt;step index="1" type="Hermitian" /&gt;
        ///     &lt;/steps&gt;
        ///     &lt;unitarians xmlns="qxml"&gt;
        ///     &lt;unitarian qubits="0" type="H" index="0" /&gt;
        ///     &lt;/unitarians&gt;
        ///     &lt;hermitians xmlns="qxml"&gt;
        ///     &lt;hermitian qubits="0" type="X" index="1" /&gt;
        ///     &lt;/hermitians&gt;
        ///     &lt;description xmlns="qxml"&gt;A single coin flip simulation.&lt;/description&gt;
        ///     &lt;/quantumCircuit&gt;
        ///
        /// </remarks>
        /// <response code="204">The quantum circuit given the qxml has been created successfully</response>
        /// <response code="400">The given qxml could not be serialized</response>
        /// <response code="409">A quantum circuit with the given name exists already</response>
        [HttpPost]
        [Route("quantum-circuits/qxml")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes(MediaTypeNames.Text.Plain)]
        public async Task<ActionResult> CreateQuantumCircuitQxmlAsync(
            [Required, FromBody] string qxml)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(QuantumCircuit));
                using var stringReader = new StringReader(qxml);
                var circuit = (QuantumCircuit)(serializer.Deserialize(stringReader) ?? throw new InvalidOperationException());

                var allCircuits = await this.database.QuantumCircuits.ReadAllAsync();
                if (allCircuits.Any(c => c.Name == circuit.Name))
                {
                    return this.Conflict($"A quantum circuit with the name {circuit.Name} exists already");
                }

                await this.database.QuantumCircuits.CreateAsync(circuit);

                return this.NoContent();
            }
            catch
            {
                return this.BadRequest($"The qxml string could not be serialized");
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
