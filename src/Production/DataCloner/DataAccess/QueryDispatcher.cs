﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using DataCloner.Interface;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;

using Murmur;

namespace DataCloner.DataAccess
{
    internal class QueryDispatcher : IQueryDispatcher
    {
        private Configuration _cache;
        private Dictionary<Int16, IQueryProvider> _providers;

        public QueryDispatcher()
        {
        }

        public void Initialize(string cacheName = Configuration.CacheName)
        {
            string fullCacheName = cacheName + Configuration.Extension;
            string fullConfigName = cacheName + ConfigurationXml.Extension;
            bool cacheIsGood = false;

            _cache = new Configuration();

            if (!File.Exists(fullConfigName))
                throw new FileNotFoundException("The configuration file doesn't exist!");

            //Hash user config
            HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            byte[] configFile = File.ReadAllBytes(fullConfigName);
            string hashConfigFile = Encoding.Default.GetString(murmur.ComputeHash(configFile));

            //Check if cached file match with config file version
            if (File.Exists(fullCacheName))
            {
                FileStream fsCache = null;
                BinaryReader brCache = null;

                try
                {
                    fsCache = new FileStream(fullCacheName, FileMode.Open);
                    brCache = new BinaryReader(fsCache);
                    
                    _cache.ConfigFileHash = brCache.ReadString();
                    cacheIsGood = _cache.ConfigFileHash == hashConfigFile;

                    if (cacheIsGood)
                    {
                        Configuration.DeserializeBody(brCache, _cache); //Load cache            
                        InitProviders(_cache.ConnectionStrings);
                        return;
                    }
                }
                finally
                { 
                    if (fsCache != null) fsCache.Close();
                }
            }

            //Rebuild cache
            if (!cacheIsGood)
            {
                var config = ConfigurationXml.Load(fullConfigName);
                _cache.ConfigFileHash = hashConfigFile;

                //Copy connection strings
                foreach (var cs in config.ConnectionStrings)
                    _cache.ConnectionStrings.Add(new Connection(cs.Id, cs.ProviderName, cs.ConnectionString, cs.SameConfigAsId));

                InitProviders(_cache.ConnectionStrings);

                //Start fetching each server
                foreach (var cs in config.ConnectionStrings)
                {
                    IQueryProvider provider = _providers[cs.Id];
                    string[] databases = provider.GetDatabasesName();
                    int nbDatabases = databases.Length;

                    for (int i = 0; i < nbDatabases; i++)
                    {
                        provider.GetColumns(_cache.CachedTables.LoadColumns, databases[i]);
                        provider.GetForeignKeys(_cache.CachedTables.LoadForeignKeys, databases[i]);
                    }
                }                
                _cache.CachedTables.GenerateCommands();

                //Save cache
                //var fsCache = new FileStream(fullCacheName, FileMode.Create);
                //_cache.Serialize(fsCache);
                //fsCache.Close();
            }
        }  

        /// <summary>
        /// Récupération des providers qui seront utilisés pour effectuer les requêtes
        /// </summary>
        /// <param name="config"></param>
        private void InitProviders(List<Connection> conns)
        {
            _providers = new Dictionary<short, IQueryProvider>();

            foreach (Connection conn in conns)
            {
                Type t = Type.GetType(conn.ProviderName);
                var provider = FastActivator<string, Int16>.CreateInstance(t, conn.ConnectionString, conn.Id) as IQueryProvider;
                _providers.Add(conn.Id, provider);
            }
        }

        public DataTable Select(IRowIdentifier ri)
        {
            return _providers[ri.TableIdentifier.ServerId].Select(ri);
        }

        public void Insert(ITableIdentifier ti, DataRow[] rows)
        {
            _providers[ti.ServerId].Insert(ti, rows);
        }

        public void Update(IRowIdentifier ri, DataRow[] rows)
        {
            _providers[ri.TableIdentifier.ServerId].Update(ri, rows);
        }

        public void Delete(IRowIdentifier ri)
        {
            _providers[ri.TableIdentifier.ServerId].Delete(ri);
        }

        public void Dispose()
        {
            //TODO : IDISPOSABLE
            foreach (var conn in _providers)
            {
                conn.Value.Dispose();
            }
        }
    }
}