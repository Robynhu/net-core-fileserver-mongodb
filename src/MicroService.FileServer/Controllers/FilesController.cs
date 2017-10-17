using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CrossCutting.Utils;
using Microsoft.Extensions.Configuration;

namespace MicroService.FileServer.Controllers
{
    [Route("[controller]")]
    [EnableCors("AllowAll")]
    public class FilesController : Controller
    {
        
        private readonly ILogger _logger;
        
        private readonly IFilesRepository _filesRepository;
        private readonly IConfigurationRoot _configurationRoot;

        public FilesController(ILoggerFactory loggerFactory, IFilesRepository filesRepository, IConfigurationRoot configurationRoot)
        {
            
            _filesRepository = filesRepository;
            _configurationRoot = configurationRoot;
            _logger = loggerFactory.CreateLogger<FilesController>();
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [Route("{userid}/{id}/{filename}")]
        public FileContentResult Get(string userid, string id, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;
            var file = _filesRepository.GetFile(id, userid);
            if (!file.FileName.ToLower().Equals(filename.ToLower()))
                return null;
            using (var memoryStream = new MemoryStream())
            {
                file.FileStream.CopyTo(memoryStream);

                return new FileContentResult(memoryStream.ToArray(), file.ContentType);
            }
           
        }

        // POST api/values
        [HttpPost]
        [EnableCors("AllowAll")]
        public async Task<IActionResult> Post()
        {
              var listurls = new List<dynamic>();
            try
            {

                var files = HttpContext.Request.Form.Files;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var userId = HttpContext.Request.Headers["UserId"];
                        var fileid = _filesRepository.AddFile(file.OpenReadStream(), file.FileName, file.ContentType, userId);
                        var item = new
                        {
                            url = _configurationRoot["UrlBase"] + "/"+ userId +"/" + fileid + "/" + file.FileName,
                            contentType = file.ContentType
                        };
                        listurls.Add(item);

                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Ocorreu um erro ao carregar o arquivo." + e.Message);
                return BadRequest();
            }

            var response = new
            {
                status = "success",
                files = listurls
            };
            return Ok(response);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
