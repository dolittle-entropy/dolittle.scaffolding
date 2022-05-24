using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Dolittle.Scaffolding.Services
{
    public class FileService : IFileService
    {
        public FileContentResult BuildKafkaConfiguration(
            [Required] List<IFormFile> files,
            [Required] string zipFileName,
            [Required] string solutionName,
            [Required] string environment,
            [Required] string username,
            [Required] string brokerUrl,
            [Required] string inputTopic,
            [Required] string commandTopic,
            [Required] string receiptsTopic,
            string changeEventsTopic)
        {
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
                    files.ForEach(file =>
                    {
                        var zipEntry = archive.CreateEntry(file.FileName);
                        using (var entryStream = zipEntry.Open())
                        {
                            file.CopyTo(entryStream);
                        }
                    });
                }
                zipStream.Position = 0;
                return new FileContentResult(zipStream.GetBuffer(), "application/zip");
            }
        }

        static string Capitalize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            if (data.Length < 2)
                return data.ToUpper();

            return $"{char.ToUpper(data[0])}{data.Substring(1).ToLower()}";
        }

        static string BuildInstructions(string brokerUrl, string inputTopic, string commandTopic, string receiptsTopic, string changeEventsTopic, string solutionName, string environment, string username)
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

            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().FullName!);
            var filePath = Path.Combine(fileInfo.DirectoryName!, "Content/README.md");
            var instructions = System.IO.File.ReadAllText(filePath);
            return instructions.Replace("$(config)", JsonConvert.SerializeObject(configuration, Formatting.Indented));
        }
    }
}
