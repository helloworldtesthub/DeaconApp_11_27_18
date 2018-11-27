using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;
using System.Configuration;
using Elmah;

namespace DeaconCCGManagement.Elmah
{
    public class ElmahRepository
    {
        // auth access to MS storage account
        private StorageCredentials _storageCredentials;

        // access cloud storage name 
        private CloudStorageAccount _storageAccount;

        // cloud table client
        private CloudTableClient _tableClient;

        // cloud table
        private CloudTable _table;

        // cloud table name
        string _tableName;

        public bool IsInitialized = false;

        public ElmahRepository()
        {
        }

        public bool Init()
        {
            if (IsInitialized) return true;

            bool useLocalEmulator =
              bool.Parse(ConfigurationManager.AppSettings["UseLocalStorageEmulator"]);

            _tableName = ConfigurationManager.AppSettings["ElmahTableName"];

            try
            {
                if (useLocalEmulator)
                {
                    // Parse the connection string and return a reference to the storage account.
                    _storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));
                }
                else
                {
                    string accountName = ConfigurationManager.AppSettings["AzureStorageAccountName"];
                    string key = ConfigurationManager.AppSettings["AzureStorageAccountKey"];
                    _storageCredentials = new StorageCredentials(accountName, key);
                    _storageAccount = new CloudStorageAccount(_storageCredentials, true);
                }


                // Create the table client.            
                _tableClient = _storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                _table = _tableClient.GetTableReference(_tableName);

                // Create the table if it doesn't exist.
                _table.CreateIfNotExists();

                IsInitialized = true;
            }
            catch (StorageException ex)
            {
                IsInitialized = false;

                // log caught exception with Elmah
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            catch (Exception ex)
            {
                IsInitialized = false;

                // log caught exception with Elmah
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return IsInitialized;

        }

        public bool RemoveOldElmahlogs()
        {
            if (!Init()) return false;

            return false;
        }    

    }
}