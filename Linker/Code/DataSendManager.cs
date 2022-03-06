using Linker.Code.DataBase;
using Linker.Code.IOConfig;
using Linker.Code.JsonClasses;
using Linker.Nodes;
using Linker.Triggers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Web.Http;

namespace Linker.Code
{
    class DataSendManager : IDisposable
    {
        public int MaximumSendRows { get; } = 10;
        public string deviceUuid { get; private set; }
        public string deviceName { get; private set; }
        public Uri HttpSendRecordsUri { get; private set; }
        public Uri HttpSendMedatDataUri { get; private set; }

        private HttpClient httpClient;
        private CancellationTokenSource cts;        
        
        private List<int> sendPrimaryKeys = new List<int>();   
        private string[] paths;
        private static readonly DataSendManager uniqueInstanceEager = new DataSendManager();

        bool? previusSendResult = null;
        HttpStatusCode previusHttpStatusCode;


        private Interval recordsSendInterval => AppConfig.IOConfiguration.SendRecordsToCloudInterval;

        private DataSendManager()
        { }


        public static DataSendManager Instance
        {
            get { return uniqueInstanceEager; }
        }

        public int SendAttempts { get; private set; }

        public void Initialize()
        {
            HttpSendRecordsUri = GetUri(AppConfig.IOConfiguration.HttpSendAdress);
            HttpSendMedatDataUri = GetUri(AppConfig.IOConfiguration.HttpSendMetaAdress);


            HtmlBuddy.CreateHttpClient(ref httpClient);
            cts = new CancellationTokenSource();

            
            deviceUuid = AppConfig.IOConfiguration.DeviceUuid;
            deviceName = AppConfig.IOConfiguration.DeviceName;

            SendMetaData();
            

            recordsSendInterval.Elapsed += RecordsSendInterval_Elapsed;
            recordsSendInterval.EnableInterval = true;
        }

        private void RecordsSendInterval_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendRecords();
        }

        public async void SendMetaData()
        {
            if(HttpSendMedatDataUri == null)
                return;

            try
            {                
                string jsonText = MetaDataToJson(AppConfig.CombinedChannelsList);
                System.Diagnostics.Debug.WriteLine(jsonText);

                var sendResult = await SendData(jsonText, HttpSendMedatDataUri).ConfigureAwait(false);

                if (sendResult.Code == (int)HttpStatusCode.Ok)
                {
                    SendAttempts = 0;
                }
                else
                {
                    SendAttempts += 1;  // TODO: now what?
                    if (SendAttempts > 3)
                    {

                    }
                }
            }
            catch(Exception ex)
            {
                LogBuddy.Log(this, Serilog.Events.LogEventLevel.Error, ex.Message);
            }
        }


        public async void SendRecords()
        {
            if (HttpSendRecordsUri == null)
                return;

            if (paths == null)
            {
                var tempPaths = AppConfig.CombinedChannelsList.Select(conv => conv.Behaviour.DataBaseRecord).ToList();
                tempPaths.Insert(0, "Primary_Key");
                tempPaths.Insert(1, "DateTime");
                paths = tempPaths.ToArray();
            }
            try
            {
                var recordrRowsToSend = await DatabaseManager.Instance.ConditionalLoadLastRecords(paths, MaximumSendRows, "Send_Succesfull", WhereCondition.NotIs200).ConfigureAwait(false);

                if (recordrRowsToSend.Count > 0 && recordrRowsToSend[0].Length > 2) // there is more then one row and more paths then only Primary_Key & DateTime
                {
                    string jsonText = RecordsToJson(paths, recordrRowsToSend);
                    System.Diagnostics.Debug.WriteLine(jsonText);

                    var sendResult = await SendData(jsonText, HttpSendRecordsUri).ConfigureAwait(false);

                    if ((HttpStatusCode)sendResult.Code == HttpStatusCode.Ok)
                    {
                        SendAttempts = 0;
                        sendPrimaryKeys.Clear();

                        foreach (object[] recordRow in recordrRowsToSend)
                            sendPrimaryKeys.Add(Convert.ToInt32(recordRow[0], NumberFormatInfo.InvariantInfo));

                        DatabaseManager.Instance.WriteSendResult(sendPrimaryKeys.ToArray(), (int)sendResult.Code);

                    }
                    else
                    {
                        SendAttempts += 1;  // TODO: now what?
                        if (SendAttempts > 3)
                        {

                        }
                    }
                }
                else
                {
                    LogSendResult(false, HttpStatusCode.None, "No database measurements to send to send");
                }
            }
            catch (Exception ex)
            {
                LogBuddy.Log(this, Serilog.Events.LogEventLevel.Error, ex.Message);
            }
        }


       


        private async Task<PostSendResult> SendData(string jsonToSend, Uri httpSendUri)
        {
            if (HttpSendRecordsUri != null && !string.IsNullOrWhiteSpace(jsonToSend))
            {
                try
                {
                    using (HttpStringContent httpStringContent = new HttpStringContent(jsonToSend, Windows.Storage.Streams.UnicodeEncoding.Utf8))
                    {
                        //HttpRequestResult httpRequestResult3 = await httpClient.TryPostAsync(httpSendUri, httpStringContent).AsTask(cts.Token).ConfigureAwait(false);
                        HttpResponseMessage httpRequestResult = await httpClient.PostAsync(httpSendUri, httpStringContent).AsTask(cts.Token).ConfigureAwait(false);
                        
                        if (httpRequestResult.StatusCode == HttpStatusCode.Ok)
                        {
                            
                            //string result = httpRequestResult.Content.ToString();
                            // sendResult.Message = result.Replace("<br>", Environment.NewLine);
                            //sendResult.Result = true;
                            //sendResult.Message = FindJsonResult(httpRequestResult.Content.ToString(), "errorCode") as string;

                            var result = GetJsonResult(httpRequestResult.Content.ToString());
                            LogSendResult(result);
                            //LogSendResult(true, (HttpStatusCode)sendResult.Code, $"Http post succesfull with status code: {sendResult.Code}");
                            return result;
                        }
                        else
                            LogSendResult(false, HttpStatusCode.None, "Http post failed");
                    }
                }
                catch (TaskCanceledException ex)
                {
                    LogSendResult(false, HttpStatusCode.None, $"Http post cancled, reason: {ex.Message}");
                }
            }
            return new PostSendResult();
        }

        


        /// <summary>
        /// Logs changes in success
        /// </summary>
        /// <param name="success">Send succesfull</param>
        /// <param name="message">the Message</param>
        private void LogSendResult(bool success, HttpStatusCode httpStatusCode, string message)
        {
            if (success == previusSendResult && previusHttpStatusCode == httpStatusCode)
                return;

            previusSendResult = success;
            previusHttpStatusCode = httpStatusCode;

            var logEventLevel = success ? Serilog.Events.LogEventLevel.Information : Serilog.Events.LogEventLevel.Error;

            LogBuddy.Log(this, logEventLevel, message);
        }

        /// <summary>
        /// Logs changes in success
        /// </summary>
        private void LogSendResult(PostSendResult result)
        {
            if (result != null)
                LogSendResult(result.Result, (HttpStatusCode)result.Code, result.Message);
            else
                LogBuddy.Log(this, Serilog.Events.LogEventLevel.Fatal, "PostSendResult is null");
        }



        /// <summary>
        /// create Json from db data
        /// </summary>
        public string MetaDataToJson(IList<MeasureNode> measureNodes)
        {
            StringWriter sw = new StringWriter();
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                //writer.Formatting = Formatting.Indented;
                // {
                writer.WriteStartObject();
                // "name" : "Jerry"
                writer.WritePropertyName(nameof(deviceUuid));
                writer.WriteValue(deviceUuid);

                writer.WritePropertyName(nameof(deviceName));
                writer.WriteValue(deviceName);
                #region Example
                // "likes": ["Comedy", "Superman"]
                //writer.WritePropertyName("likes");
                //writer.WriteStartArray();
                //foreach (string like in p.Likes)
                //{
                //    writer.WriteValue(like);
                //}
                //writer.WriteEndArray();

                // "likes": ["Comedy", "Superman"]
                #endregion

                writer.WritePropertyName("data");
                writer.WriteStartArray();
                foreach (MeasureNode nodeItem in measureNodes)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("id");
                    writer.WriteValue(nodeItem.Behaviour.DataBaseRecord);

                    writer.WritePropertyName("key");
                    writer.WriteValue(nodeItem.Name);

                    writer.WritePropertyName("unit");
                    writer.WriteValue(nodeItem.Unit);

                    writer.WriteEndObject();
                }


                writer.WriteEndArray();

                // }
                writer.WriteEndObject();
            }

            return sw.ToString();
        }


        /// <summary>
        /// create Json from db data
        /// </summary>
        public string RecordsToJson(string[] paths, List<object[]> result)
        {
            StringWriter sw = new StringWriter();
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                //writer.Formatting = Formatting.Indented;
                // {
                writer.WriteStartObject();
                // "name" : "Jerry"
                writer.WritePropertyName(nameof(deviceUuid));
                writer.WriteValue(deviceUuid);

                writer.WritePropertyName(nameof(deviceName));
                writer.WriteValue(deviceName);
                #region Example
                // "likes": ["Comedy", "Superman"]
                //writer.WritePropertyName("likes");
                //writer.WriteStartArray();
                //foreach (string like in p.Likes)
                //{
                //    writer.WriteValue(like);
                //}
                //writer.WriteEndArray();

                // "likes": ["Comedy", "Superman"]
                #endregion

                writer.WritePropertyName("data");
                writer.WriteStartArray();
                foreach (object[] row in result)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("rowNr");
                    writer.WriteValue(row[0]);

                    writer.WritePropertyName("timeStamp");
                    writer.WriteValue(row[1]);

                    writer.WritePropertyName("rowData");
                    writer.WriteStartArray();

                    for (int i = 2; i < row.Length; i++)
                    {
                        writer.WriteStartObject();
                        
                        writer.WritePropertyName("itemId");
                        writer.WriteValue(paths[i]);

                        writer.WritePropertyName("value");
                        writer.WriteValue(DBNull.Value.Equals(row[i]) ? string.Empty : row[i]);

                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                // }
                writer.WriteEndObject();
            }

            return sw.ToString();
        }


        

        /// <summary>
        /// Searches for the suplied Json propertyname and return the result
        /// </summary>
        /// <param name="JsonPropertyName">The name of the property (case insensitive)</param>
        /// <returns>the value of the property</returns>
        public static object FindJsonResult(string JsonString, string JsonPropertyName)
        {
            using (var reader = new JsonTextReader(new StringReader(JsonString)))
                while (reader.Read())
                    if (reader.TokenType == JsonToken.PropertyName)
                        if (reader.Value.ToString().Equals(JsonPropertyName, StringComparison.OrdinalIgnoreCase))
                            if (reader.Read())
                                return reader.Value;
            return null;
        }

        private static PostSendResult GetJsonResult(string JsonString)
        {
            bool succesResult = false;
            int errorCode = -1;
            string message = string.Empty;
            bool parseSucceeded;
            using (var reader = new JsonTextReader(new StringReader(JsonString)))
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        string jsonValue = reader.Value.ToString();
                        reader.Read();
                        switch (jsonValue)
                        {
                            case "success":
                                parseSucceeded = bool.TryParse(reader.Value.ToString(), out succesResult);
                                break;
                            case "errorCode":
                                parseSucceeded = int.TryParse(reader.Value.ToString(), out errorCode);
                                break;
                            case "message":
                                message = reader.Value as string;
                                break;
                            default:
                                break;
                        }
                    }
                }
            return new PostSendResult(succesResult, errorCode, message);
        }


        /// <summary>
        /// returns a Uri based on a path, logs the result
        /// </summary>
        private Uri GetUri(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                Uri returnUri;
                if (HtmlBuddy.TryGetUri(path, out returnUri))
                    LogBuddy.Log(this, Serilog.Events.LogEventLevel.Information, $"Succesfull initialized HTML post service, adress: {path} valid");
                else
                    LogBuddy.Log(this, Serilog.Events.LogEventLevel.Fatal, $"Failed to initialize HTML post service, adress: {path} invalid");
                return returnUri;
            }
            else
                LogBuddy.Log(this, Serilog.Events.LogEventLevel.Fatal, $"IOConfiguration HttpSendAdress empty, failed to initialize HTML post");
            return null;
        }


        public enum ActionAferSend
        {
            RemoveRecords,
            RemoveAfterWeek,
            KeepRecords
        }


        public enum SendResult
        {
            Succes,
            Failed,
            Unknown
        }

        public void Dispose()
        {
            cts.Dispose();
            httpClient.Dispose();
        }
    }


    public class PostSendResult
    {
        public bool Result { get; set; }
        public int Code { get; set; }

        public string Message { get; set; }

        public PostSendResult(bool result, int code, string message)
        {
            Result = result;
            Code = code;
            Message = message;
        }

        public PostSendResult(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public PostSendResult()
        {
            Code = -1;
            Message = string.Empty;
        }
    }


}
