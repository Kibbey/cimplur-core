using System;
using Domain.Entities;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repository
{



    public class BaseService : IDisposable
    {
        private StreamContext context;
        protected StreamContext Context
        {
            get
            {
                if (context == null)
                {
                    /*
                    IConfigurationRoot configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(@Directory.GetCurrentDirectory() + "/appsettings.json").Build();*/
                    var builder = new DbContextOptionsBuilder<StreamContext>();
                    var connectionString = Environment.GetEnvironmentVariable("DatabaseConnection");
                    //var connectionString = configuration.GetConnectionString("DatabaseConnection");
                    builder.UseSqlServer(connectionString);
                    context = new StreamContext(builder.Options);
                }
                return context;
            }
        }


        protected static bool InProduction
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["Production"] ?? "false");
            }
        }

        protected static string BucketName { get { return "cimplur"; } }
        protected static string BucketNameThumb { get { return "cimplurthumbs"; } }



        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
            }
        }
    }
}
