using Dolittle.Scaffolding.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DolittleScaffolding.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class FileController : ControllerBase
    {
        static string[] REQUIRED_FILES = new[] { "accessKey.pem", "certificate.pem", "ca.pem" };
        readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Scaffold a Kafka configuration environment for the developer and produce a downloadable Zip file with everything they need
        /// </summary>
        /// <param name="files">specifically, ca.pem, certificate.pem and accessKey.pem. These files must be present</param>
        /// <param name="zipFileName">Name of the zipfile to download</param>
        /// <param name="solutionName">Short name of your solution, i.e. not "Taco Empire 2.0" but rather "te2"</param>
        /// <param name="environment">One of dev|test|prod</param>
        /// <param name="username">first or last name of the user, no spaces!</param>
        /// <param name="brokerUrl">Value pointing to the kafka broker URL</param>
        /// <param name="inputTopic">Input topic definition</param>
        /// <param name="commandTopic">Command topic definition</param>
        /// <param name="receiptsTopic">Receipt topic definition</param>
        /// <param name="changeEventsTopic">Change Eventstopic definition</param>
        /// <returns></returns>
        [HttpPost(nameof(GetKafkaConfiguration))]
        public IActionResult GetKafkaConfiguration(
            [Required] List<IFormFile> files,
            [Required] string zipFileName,
            [Required] string solutionName,
            [Required] string environment,
            [Required] string username,
            [Required] string brokerUrl,
            [Required] string inputTopic,
            [Required] string commandTopic,
            [Required] string receiptsTopic,
            string changeEventsTopic
            )
        {
            if (files.Count == 0)
                return BadRequest("No files uploaded");

            if (string.IsNullOrEmpty(zipFileName.Trim()))
                return BadRequest("No Zip filename given");

            if (string.IsNullOrEmpty(solutionName.Trim()))
                return BadRequest("Missing solution name");

            if (string.IsNullOrEmpty(environment.Trim()))
                return BadRequest("Environment name missing");

            if (string.IsNullOrEmpty(username.Trim()))
                return BadRequest("Missing username");

            foreach (var requiredFile in REQUIRED_FILES)
            {
                if (!files.Any(file => file.FileName.Equals(requiredFile, StringComparison.InvariantCultureIgnoreCase)))
                    return BadRequest($"File '{requiredFile}' is missing");
            }

            if (!zipFileName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                zipFileName += ".zip";

            try
            {
                var result = _fileService.BuildKafkaConfiguration(files, zipFileName, solutionName, environment, username, brokerUrl, inputTopic, commandTopic, receiptsTopic, changeEventsTopic);
                return File(result.FileContents, result.ContentType, zipFileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

