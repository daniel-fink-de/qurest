using QuRest.Application.Abstractions;
using QuRest.Application.Interfaces;
using QuRest.Domain;
using System.Threading.Tasks;

namespace QuRest.Application.Services
{
    public class QxmlDrawer : IQxmlDrawer
    {
        private readonly IQxmlTranslator qxmlTranslator;
        private readonly IQuantumProgrammingStudioService quantumProgrammingStudioService;

        public QxmlDrawer(IQxmlTranslator qxmlTranslator,
            IQuantumProgrammingStudioService quantumProgrammingStudioService)
        {
            this.qxmlTranslator = qxmlTranslator;
            this.quantumProgrammingStudioService = quantumProgrammingStudioService;
        }

        public async Task<string> DrawAsync(string qasm)
        {
            var svg = await this.quantumProgrammingStudioService.QasmToSvg(qasm);
            return svg;
        }

        public async Task<string> DrawAsync(QuantumCircuit quantumCircuit)
        {
            return await this.DrawAsync(await this.qxmlTranslator.TranslateToQasmAsync(quantumCircuit));
        }
    }
}