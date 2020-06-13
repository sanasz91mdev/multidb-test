using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Npgsql;

namespace dbManager
{
    public static class manager
    {
        public static EntityConnection GetConnectionString(object EntityName)
        {
            //YhG0j6YPoCtgND/RMckmpg==
            string InitialCatalog = "profile";
            //string DataSource = WebConfigEncryption.Decrypt(ConfigurationManager.AppSettings["DataSource"].ToString());
            string UserID = "postgres";
            string Password = "tpstps_1";
            string SchemaName = "user";
            Dictionary<string, string> Dict_EntitySchema = new Dictionary<string, string>();
            NpgsqlConnectionStringBuilder sb = new NpgsqlConnectionStringBuilder();
            sb.Host = "localhost";
            sb.Port = 5432;
            sb.Database = "iristest";
            sb.Username = "postgres";
            sb.Password = "tpstps_1";
            sb.Timeout = 20;
            sb.CommandTimeout = 20;

            var connString = new System.Data.EntityClient.EntityConnectionStringBuilder
            {
                Metadata = string.Format("res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", EntityName.ToString()),
                Provider = "Npgsql",                
                ProviderConnectionString = sb.ConnectionString
            };
            return CreateConnection(SchemaName, connString, EntityName.ToString(), Dict_EntitySchema);
        }

        /// <summary>
        /// Creates the EntityConnection, based on new schema & existing connectionString
        /// </summary>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="connectionBuilder"></param>
        /// <param name="modelName">Name of the model.</param>
        /// <returns></returns>
        public static EntityConnection CreateConnection(string schemaName, EntityConnectionStringBuilder connectionBuilder, string EntityName, Dictionary<string, string> Dict_EntitySchema)
        {
            Func<string, Stream> generateStream =
                extension => Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Concat(EntityName, extension));

            // string DefaultSchemaName = WebConfigEncryption.Decrypt(ConfigurationManager.AppSettings["SchemaName"].ToString());
            string DefaultSchemaName = "user";

            Action<IEnumerable<Stream>> disposeCollection = streams =>
            {
                if (streams == null)
                    return;

                foreach (var stream in streams.Where(stream => stream != null))
                    stream.Dispose();
            };

            var conceptualReader = generateStream(".csdl");
            var mappingReader = generateStream(".msl");
            var storageReader = generateStream(".ssdl");

            if (conceptualReader == null || mappingReader == null || storageReader == null)
            {
                disposeCollection(new[] { conceptualReader, mappingReader, storageReader });
                return null;
            }

            var storageXml = XElement.Load(storageReader);

            IEnumerable<XElement> Lst_Elements = storageXml.Descendants();

            //"metadata=res://*/dataModel.userprofile.csdl
            //|res://*/dataModel.userprofile.ssdl
            //|res://*/dataModel.userprofile.msl;
            //provider=Npgsql;
            //provider connection string=&quot;Host=localhost;Database=iristest;Username=postgres;Password=tpstps_1;Persist Security Info=True&quot;"

            if (Dict_EntitySchema != null)
            {
                if (Dict_EntitySchema.Count > 0)
                {
                    string Schema_Evaluated = string.Empty;
                    foreach (var entitySet in Lst_Elements)
                    {
                        var FirstAttribute = entitySet.Attributes("Name").FirstOrDefault();
                        var schemaAttribute = entitySet.Attributes("Schema").FirstOrDefault();
                        if (schemaAttribute != null && FirstAttribute != null)
                        {
                            Dict_EntitySchema.TryGetValue(FirstAttribute.Value, out Schema_Evaluated);
                            if (Schema_Evaluated != null)
                            {
                                //if (Schema_Evaluated.Equals(SchemaAssociations.Schema.ToString(), StringComparison.OrdinalIgnoreCase))
                                //{ schemaAttribute.SetValue(schemaName); }
                                //else if (Schema_Evaluated.Equals(SchemaAssociations.Hub.ToString(), StringComparison.OrdinalIgnoreCase))
                                //{ schemaAttribute.SetValue(DefaultSchemaName); }

                                schemaAttribute.SetValue(DefaultSchemaName);
                            }
                            else { schemaAttribute.SetValue(schemaName); }
                        }
                        else
                        {
                            if (schemaAttribute != null)
                                schemaAttribute.SetValue(schemaName);
                        }
                    }
                }
                else
                {
                    foreach (var entitySet in Lst_Elements)
                    {
                        var schemaAttribute = entitySet.Attributes("Schema").FirstOrDefault();
                        if (schemaAttribute != null)
                            schemaAttribute.SetValue(schemaName);
                    }
                }
            }
            else
            {
                foreach (var entitySet in Lst_Elements)
                {
                    var schemaAttribute = entitySet.Attributes("Schema").FirstOrDefault();
                    if (schemaAttribute != null)
                        schemaAttribute.SetValue(schemaName);
                }
            }

            storageXml.CreateReader();

            var workspace = new MetadataWorkspace();

            var storageCollection = new StoreItemCollection(new[] { storageXml.CreateReader() });
            var conceptualCollection = new EdmItemCollection(new[] { XmlReader.Create(conceptualReader) });
            var mappingCollection = new StorageMappingItemCollection(conceptualCollection,
                                                                    storageCollection,
                                                                    new[] { XmlReader.Create(mappingReader) });

            workspace.RegisterItemCollection(conceptualCollection);
            workspace.RegisterItemCollection(storageCollection);
            workspace.RegisterItemCollection(mappingCollection);

            var connection = DbProviderFactories.GetFactory(connectionBuilder.Provider).CreateConnection();
            if (connection == null)
            {
                disposeCollection(new[] { conceptualReader, mappingReader, storageReader });
                return null;
            }

            connection.ConnectionString = connectionBuilder.ProviderConnectionString;
            return new EntityConnection(workspace, connection);
        }

    }
}
