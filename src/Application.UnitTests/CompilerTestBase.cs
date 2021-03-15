using Microsoft.Extensions.DependencyInjection;
using QuRest.Application.Abstractions;
using System;

namespace QuRest.Application.UnitTests
{
    public abstract class CompilerTestBase
    {
        private readonly IServiceProvider serviceProvider;

        protected IQxmlCompiler Compiler => this.serviceProvider.GetService<IQxmlCompiler>() 
                                            ?? throw new ApplicationException("Dependency injection failed.");

        protected CompilerTestBase()
        {
            this.serviceProvider = new ServiceCollection()
                .AddApplication()
                .BuildServiceProvider();
        }
    }
}
