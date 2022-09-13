using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT;
using TwinCAT.TypeSystem;
using TwinCAT.Ads.TypeSystem;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;

namespace DacConfigManager
{
    public class DacConfigManager
    {


        public static void Main(string[] args)
        {
            string helpStr = "Arguments: command AmsNetID Port ConfigFile. Examples:\nDacConfigManager.exe read \"127.0.0.1.1.1\" 851 configFile.xml\nDacConfigManager.exe write \"127.0.0.1.1.1\" 852 configFile.xml ";
            if (args.Length < 3)
                Console.WriteLine(helpStr);
            else
            {
                DacConfigManager manager;
                switch (args[0].ToLower())
                {
                    case "write":
                        if (args.Length > 3)
                        {
                            manager = new(args[1], int.Parse(args[2]), args[3]);
                            manager.WriteToPLC();
                        }
                        else
                            Console.WriteLine("config file is required.\n" + helpStr);
                        break;
                    case "read":
                        manager = new(args[1], int.Parse(args[2]), args.Length > 3 ? args[0] : "");
                        manager.ReadFromPLC();
                        break;
                    default:
                        Console.WriteLine(helpStr);
                        break;
                }
            }
        }

        public DacConfigManager(string amsNetID, int port, string confFilePath = "")
        {
            AmsNetIDStr = amsNetID;
            Port = port;
            ConfFilePath = confFilePath;
        }

        public void ReadFromPLC()
        {
            Console.WriteLine("Reading from PLC...");
            readFromTarget(AmsNetIDStr, Port);
            Console.WriteLine("Writing to file...");
            writeToFile(ConfFilePath);
            Console.WriteLine("Complete!");
        }

        public void WriteToPLC()
        {
            Console.WriteLine("Reading from file...");
            readFromFile(ConfFilePath);
            Console.WriteLine("Writing to PLC...");
            writeToTarget(AmsNetIDStr, Port);
            Console.WriteLine("Complete!");
        }

        ~DacConfigManager()
        {
            if(adsClient != null)
                adsClient.Dispose();
        }
        
        


        private bool writeToFile(string path = "configFile.xml")
        {
            if (path == "")
                path = "configFile.xml";
            try
            {
                using (var writer = XmlWriter.Create(path, new XmlWriterSettings() { Indent = true }))
                {
                    XmlSerializer serializer = new XmlSerializer(serializabaleSymbols.GetType());
                    serializer.Serialize(writer, serializabaleSymbols);
                }
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            
        }


        private bool writeToTarget(string amsNetID, int port)
        {
            try
            {
                using (adsClient = new())
                {
                    adsClient.Connect(amsNetID, port);
                    foreach(var symbol in serializabaleSymbols)
                    {
                        adsClient.WriteValue(symbol.Path, symbol.Value);
                    }
                    return true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                return false;
            }
        }


        private bool readFromTarget(string amsNetID, int port)
        {
            try
            {
                using(adsClient = new())
                {
                    adsClient.Connect(amsNetID, port);
                    IDynamicSymbolLoader loader = (IDynamicSymbolLoader)SymbolLoaderFactory.Create(adsClient, SymbolLoaderSettings.DefaultDynamic);
                    dynamic symbols = loader.SymbolsDynamic;
                    Func<ISymbol, bool> selector = (s) => s.IsPersistent && (s.Category == DataTypeCategory.Primitive || s.Category == DataTypeCategory.String || s.Category == DataTypeCategory.Enum);
                    SymbolIterator iterator = new(symbols, true, selector);
                    //Func<ISymbol, bool> selector2 = (s) => s.IsPersistent;
                    //SymbolIterator iterator2 = new(symbols, true, selector2);

                    foreach (DynamicSymbol symbol in iterator)
                    {
                        if (symbol.Category == DataTypeCategory.Enum)
                            Console.ReadKey();
                        serializabaleSymbols.Add(new SerializableValueSymbol(symbol));
                    }
                    return true;
                }
                
            }
            catch(Exception ex)
            {
                Console.WriteLine(string.Format("{0}\n{1}",ex.Message, ex.StackTrace));
                return false;
            }
        }

        private bool readFromFile(string path)
        {
            try
            {
                XmlSerializer serializer = new(serializabaleSymbols.GetType());
                using (TextReader reader = new StreamReader(path))
                {
                    dynamic result = serializer.Deserialize(reader);
                    if (result != null)
                        serializabaleSymbols = result;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        private List<SerializableValueSymbol> serializabaleSymbols = new();
        private AdsClient adsClient = new();

        public string AmsNetIDStr { get; set; }
        public int Port { get; set; }
        public string ConfFilePath { get; set; }
    }
}
