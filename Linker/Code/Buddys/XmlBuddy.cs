using Microsoft.Toolkit.Uwp.Helpers;
using Linker.Channels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Serilog.Events;
using Linker.Code.Behaviours;

namespace Linker
{
    /// <summary>
    /// Your buddy for converting object to and from XML
    /// </summary>
    public static class XmlBuddy
    {

        public static async Task SaveObjectToXml<T>(T objectToSave, string fileName)
        {
            StorageFolder folder = GetDocumentsStorageFolder();
            var createFileTask = folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                       
            var typeList = new List<Type>();
            typeList.AddRange(Channel.DerivedTypes);
            typeList.AddRange(Behaviour.DerivedTypes);

            StorageFile dirtyBasterd = await createFileTask;
            var openStreamTask = dirtyBasterd.OpenStreamForWriteAsync().ConfigureAwait(true);;

            var serializer = new XmlSerializer(typeof(T), typeList.ToArray());

            Stream stream = await openStreamTask;
            
            using (stream)
            {
                serializer.Serialize(stream, objectToSave);
            }
        }


        public static async Task<T> LoadObjectFromXml<T>(string fileName)
        {
            StorageFolder folder = GetDocumentsStorageFolder();
            bool fileExists = await StorageFileHelper.FileExistsAsync(folder, fileName, false);

            if (fileExists)
            {                
                T objectFromXml = default(T);
                var typeList = new List<Type>();
                typeList.AddRange(Channel.DerivedTypes);
                typeList.AddRange(Behaviour.DerivedTypes);
                var serializer = new XmlSerializer(typeof(T));
                StorageFile file = await folder.GetFileAsync(fileName);
                Stream stream = await file.OpenStreamForReadAsync();
                objectFromXml = (T)serializer.Deserialize(stream);
                stream.Dispose();
                return objectFromXml;
            }
            return default(T);
        }





            private static StorageFolder GetDocumentsStorageFolder()
            {
                StorageFolder appDocumentsLibrary = null;
                try
                {
                    appDocumentsLibrary = KnownFolders.DocumentsLibrary;

                    if (ApplicationData.Current.LocalSettings.Values["folderToken"] == null)
                    {
                        string folderToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(appDocumentsLibrary);
                        ApplicationData.Current.LocalSettings.Values["folderToken"] = folderToken;
                    }
                }
                catch (FileNotFoundException ex)
                {
                    LogBuddy.Log("Error accessing documentsfolder: " + ex.Message, LogEventLevel.Error);
                }
                return appDocumentsLibrary;
            }




        }
    }
