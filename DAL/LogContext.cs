using System;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using ImageSharingWithCloud.Models;
using ImageSharingWithCloud.Models.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ImageSharingWithCloud.DAL
{
    public class LogContext : ILogContext
    {
        private TableClient _tableClient;

        private ILogger<LogContext> logger;

        public LogContext(IConfiguration configuration, ILogger<LogContext> logger)
        {
            this.logger = logger;

            Uri logTableServiceUri = null;
            string logTableName = null;
            /*
             * TODO Get the table service URI and table name.
             */
            logger.LogInformation("Looking up Storage URI... ");
            
            logger.LogInformation("Using Table Storage URI: " + logTableServiceUri);
            logger.LogInformation("Using Table: " + logTableName);
            
            // Access key will have been loaded from Secrets (Development) or Key Vault (Production)
            TableSharedKeyCredential credential = new TableSharedKeyCredential(
                configuration[StorageConfig.LogEntryDbAccountName],
                configuration[StorageConfig.LogEntryDbAccessKey]);

            logger.LogInformation("Initializing table client....");
            // TODO Set the table client for interacting with the table service (see TableClient constructors)

            logger.LogInformation("....table client URI = " + _tableClient.Uri);
        }


        public async Task AddLogEntryAsync(string userId, string userName, ImageView image)
        {
            var entry = new LogEntry(userId, image.Id)
            {
                Username = userName,
                Caption = image.Caption,
                ImageId = image.Id,
                Uri = image.Uri
            };

            logger.LogDebug("Adding log entry for image: {0}", image.Id);

            Response response = null;
            // TODO add a log entry for this image view

            if (response.IsError)
            {
                logger.LogError("Failed to add log entry, HTTP response {response}", response.Status);
            } 
            else
            {
                logger.LogDebug("Added log entry with HTTP response {response}", response.Status);
            }

        }

        public AsyncPageable<LogEntry> Logs(bool todayOnly = false)
        {
            if (todayOnly)
            {
                // TODO just return logs for today
                return null;
            }
            else
            {
                return _tableClient.QueryAsync<LogEntry>(logEntry => true);
            }
        }

    }
}