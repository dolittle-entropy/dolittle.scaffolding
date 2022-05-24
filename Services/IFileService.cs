using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dolittle.Scaffolding.Services
{
    public interface IFileService
    {
        FileContentResult BuildKafkaConfiguration(
            [Required] List<IFormFile> files,
            [Required] string zipFileName,
            [Required] string solutionName,
            [Required] string environment,
            [Required] string username,
            [Required] string brokerUrl,
            [Required] string inputTopic,
            [Required] string commandTopic,
            [Required] string receiptsTopic,
            string changeEventsTopic);
    }
}
