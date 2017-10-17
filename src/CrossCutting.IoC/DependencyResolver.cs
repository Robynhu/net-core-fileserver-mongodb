using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Repository;
using Infra.Data.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace CrossCutting.IoC
{
    public static class DependencyResolver
    {
        public static void ConfigurarIoC(IServiceCollection service)
        {
            service.AddTransient<IFilesRepository, FileRepository>();
           
        }

    }
}
