using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Reflection;

namespace DolittleScaffolding.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class FileController : ControllerBase
    {
        static string[] REQUIRED_FILES = new[] { "accessKey.pem", "certificate.pem", "ca.pem" };

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

            if (!zipFileName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                zipFileName += ".zip";

            foreach (var requiredFile in REQUIRED_FILES)
            {
                if (!files.Any(file => file.FileName.Equals(requiredFile, StringComparison.InvariantCultureIgnoreCase)))
                    return BadRequest($"File '{requiredFile}' is missing");
            }

            using (var zipStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    var readmeEntry = archive.CreateEntry("README.md");
                    using (var readmeEntryStream = readmeEntry.Open())
                    using (var streamWriter = new StreamWriter(readmeEntryStream))
                    {
                        streamWriter.Write(BuildInstructions(brokerUrl, inputTopic, commandTopic, receiptsTopic, changeEventsTopic, solutionName, environment, username));
                    }

                    var configurationBuilder = archive.CreateEntry("KafkaConfigurationBuilder.cs");
                    using (var readmeEntryStream = configurationBuilder.Open())
                    using (var streamWriter = new StreamWriter(readmeEntryStream))
                    {
                        var codeFileContents = System.IO.File.ReadAllText("Content/KafkaConfigurationBuilder.cs");
                        codeFileContents = codeFileContents.Replace("$(solutionName)", Capitalize(solutionName));
                        streamWriter.Write(codeFileContents);
                    }

                    foreach (var file in files)
                    {
                        var zipEntry = archive.CreateEntry(file.FileName);
                        using (var entryStream = zipEntry.Open())
                        {
                            file.CopyTo(entryStream);
                        }
                    }
                }
                zipStream.Position = 0;
                return File(zipStream.GetBuffer(), "application/zip", zipFileName);
            }
        }

        string Capitalize(string data)
        {
            if (data.Length < 2)
                return data.ToUpper();

            return $"{char.ToUpper(data[0])}{data.Substring(1).ToLower()}";
        }

        string BuildInstructions(string brokerUrl, string inputTopic, string commandTopic, string receiptsTopic, string changeEventsTopic, string solutionName, string environment, string username)
        {
            var configuration = new KafkaConfiguration(
                new Kafka(
                    $"{solutionName}-{environment}-{username}",
                    brokerUrl,
                    inputTopic,
                    commandTopic,
                    receiptsTopic,
                    changeEventsTopic,
                    new Ssl("<path>/ca.pem", "<path>/certificate.pem", "<path>/accessKey.pem")
                ));
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().FullName);
            var filePath = Path.Combine(fileInfo.DirectoryName, "Content/README.md");
            var instructions = System.IO.File.ReadAllText(filePath);
            return instructions.Replace("$(config)", JsonConvert.SerializeObject(configuration, Formatting.Indented));
        }
    }
}

public record KafkaConfiguration(Kafka Kafka);
public record Kafka(string GroupId, string BrokerUrl, string InputTopic, string CommandTopic, string ReceiptsTopic, string ChangeEventsTopic, Ssl Ssl);
public record Ssl(string Authority, string Certificate, string Key);
