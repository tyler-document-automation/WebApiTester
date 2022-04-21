using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace WebApiTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static HttpClient client;

        //string defaultURL = "http://localhost:23760/";
        string defaultURL = $"http://{Environment.MachineName}:8037/";

        /// <summary>
        /// web api client set up and initialize user interface 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            defaultURL = Properties.Settings.Default.url;
            WebApiTextbox.Text = defaultURL;
            ApiKeyTextbox.Text = Properties.Settings.Default.apikey;
            BatchIDTextbox.Text = Properties.Settings.Default.batchids;
            MaxItemsTextbox.Text = Properties.Settings.Default.maxItems.ToString();
            AllDataCheckBox.IsChecked = Properties.Settings.Default.alldata;

            if (string.IsNullOrWhiteSpace(defaultURL))
            {
                defaultURL = $"http://{Environment.MachineName}:8037/";
                WebApiTextbox.Text = defaultURL;
            }
           
            StateTextbox.Items.Add("Any");
            StateTextbox.Items.Add("Completed");
            StateTextbox.Items.Add("InFlight");
            StateTextbox.Items.Add("Error");
            StateTextbox.Items.Add("Cancelled");
            StateTextbox.Items.Add("Purged");
            StateTextbox.Items.Add("Archived");

            StateTextbox.Text = Properties.Settings.Default.batch_status;

            var logfile = Path.Combine(Environment.CurrentDirectory, "logs", "webapitool-.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(logfile, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            SetupWebClient(defaultURL, true, false);
        }

        public async void SetupWebClient(string url, bool showOkStatus, bool getWithBody)
        {
            var apiKey = ApiKeyTextbox.Text;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ApiKeyTextbox.Focus();
                WebApiStatus.Text = "No API key";
                MessageBox.Show("PROVIDE A VALID API KEY", this.Title);
                return;
            }

            var handler = new HttpClientHandler
            {
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };
            client = new HttpClient(handler) {BaseAddress = new Uri(url)};

            // Environment.MachineName
            WebApiTextbox.Text = client.BaseAddress.ToString();
            ApiKeyTextbox.Text = apiKey;

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("api_key", apiKey);

            if (!showOkStatus) return;
            var sw = CallApiStart();
            
            try
            {
                Slog.ApiCall(client, "/api/Batch/DefinitionNames");
                HttpResponseMessage response = await client.GetAsync("/api/Batch/DefinitionNames");
                CallApiEnd(sw);
                WebApiStatus.Text = response.ReasonPhrase;
            }
            catch (Exception e)
            {
                CallApiEnd();
                NoticeTextbox.Text = $"Failed to establish connection with web api. \nPlease double check url.\r\n{e.Message}\r\n{e.StackTrace}";
            }
        }

        /// <summary>
        /// Create Batch call
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_CreateBatch(object sender, RoutedEventArgs e)
        {
            var Page2 = new CreateBatch(); //create your new form.
            Page2.ShowDialog(); //show the new form.
            Page2.Close();
        }

        /// <summary>
        /// Batch Status call
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_RetrieveBatch(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                int batchID = 0;
                bool canConvert = int.TryParse(BatchIDTextbox.Text, out batchID);
                if (!canConvert)
                {
                    NoticeTextbox.Text = "The input BatchID is not a valid integer.";
                    return;
                }


                var includeExtraction = IncludeExtractionCheckBox.IsChecked??false; // If set to be true, include extraction data
                var includeRedaction = IncludeRedactionCheckBox.IsChecked??false; // If set to be true, include redaction data
                var includeFileReleaseData = IncludeFileReleaseDataCheckBox.IsChecked??false; // if file release data is true, you will see release file paths and/or stream 
                var includeOrg = IncludeOrgCheckBox.IsChecked??false; // If set to be true, include original file
                var includeOcr = IncludeOcrCheckBox.IsChecked??false; // If set to be true, include Ocr information
                var includeStream = SavePackageCheckBox.IsChecked??false; // if stream is set to true, you will save the package as a zipped file. Otherwise, display filepath only.
                var includeRotPages = RotatePagesCheckBox.IsChecked??false; // Retrieve rotate pages
                string result = "";

                string url = ((MainWindow) Application.Current.MainWindow).WebApiTextbox.Text;
                SetupWebClient(url, false, false);
                try
                {
                    var apiUrl = $"api/Batch/Retrieve/{batchID}?" +
                                 $"noRedaction={!includeRedaction}&" +
                                 $"noExtraction={!includeExtraction}&" +
                                 $"noOriginalFile={!includeOrg}&" +
                                 $"noOcr={!includeOcr}&" +
                                 $"fileStream={includeStream}&" +
                                 $"rotatePages={includeRotPages}&" +
                                 $"noFiles={!includeFileReleaseData}";

                    WebApiStatus.Text = "Processing...";
                    var sw = CallApiStart();
                    Slog.ApiCall(client, apiUrl);
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    result = await response.Content.ReadAsStringAsync();
                    CallApiEnd(sw);

                    if (response.IsSuccessStatusCode)
                    {
                        if (includeStream)
                        {
                            var body = JObject.Parse(result);
                            if (body["filePackage"] != null && body["filePackage"].Type != JTokenType.Null &&
                                JObject.Parse(body["filePackage"].ToString())["package"] != null)
                            {
                                var package = JObject.Parse(body["filePackage"].ToString())["package"];
                                byte[] zippedPackage = Convert.FromBase64String(package.ToString());
                                string path = Path.GetTempPath();
                                File.WriteAllBytes(@path + $"\\Batch_{batchID}_Release_Package.zip", zippedPackage);
                                FileInfo file = new FileInfo(@path + $"\\Batch_{batchID}_Release_Package.zip");

                                HttpResponseMessage responseWithoutStream = await client.GetAsync(apiUrl);

                                //HttpResponseMessage responseWithoutStream = await client.GetAsync(
                                //    $"api/Batch/Retrieve/{batchID}?" +
                                //    $"noRedaction={!includeRedaction}&" +
                                //    $"noExtraction={!includeExtraction}&" +
                                //    $"noOriginalFile={!includeOrg}&" +
                                //    $"noOcr={!includeOcr}&" +
                                //    "fileStream=false&" +
                                //    $"noFiles={!includeFileReleaseData}");
                                var resultWithoutStream = await responseWithoutStream.Content.ReadAsStringAsync();


                                NoticeTextbox.Text = "The zipped release package is saved to the local machine" +
                                                     $"\nZip file path: \n{file.FullName}\n\n" +
                                                     JValue.Parse(resultWithoutStream).ToString(Formatting.Indented);
                            }
                            else
                            {
                                NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);

                            }
                        }
                        else
                        {
                            NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);

                        }
                    }
                    else
                    {
                        NoticeTextbox.Text = response.ReasonPhrase + "\n" + result;
                    }
                }
                catch (Exception ex)
                {
                    CallApiEnd();
                    NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url, api Key and parameters";
                }
            }
        }


        private async void Button_BatchStatus(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                string result = "";
                List<int> batchIdList = new List<int>();

                string[] stringBatchIds = BatchIDTextbox.Text.Split(',');

                foreach (var stringBatchId in stringBatchIds)
                {
                    if (string.IsNullOrWhiteSpace(stringBatchId)) continue;
                    int batchID = 0;
                    bool canConvert = int.TryParse(stringBatchId, out batchID);
                    if (!canConvert)
                    {
                        NoticeTextbox.Text = "The input BatchID array contains one or more invalid integers.";
                        return;
                    }

                    batchIdList.Add(batchID);
                }

                string workflow = WorkflowTextbox.Text;
                string state = StateTextbox.Text;
                string maxItems = MaxItemsTextbox.Text;
                bool allData = AllDataCheckBox.IsChecked??true;
                //string batchIdParams = JsonConvert.SerializeObject(intBatchIds);
                int workflowId = 0;

                if (!string.IsNullOrWhiteSpace(workflow))
                {
                    bool canConvert = int.TryParse(WorkflowTextbox.Text, out workflowId);
                    if (!canConvert)
                    {
                        NoticeTextbox.Text = "The input workflowID is not a valid integer.";
                        return;
                    }
                }

                string url = ((MainWindow) Application.Current.MainWindow).WebApiTextbox.Text;
                var batchIdUrl = "";
                foreach (var batchId in batchIdList)
                {
                    batchIdUrl += $"&batchIds={batchId}";
                }

                SetupWebClient(url, false, false);

                try
                {
                    var sw = CallApiStart();
                    var requestUrl = $"api/Batch/Status?workflowId={workflowId}&state={state}&maxitems={maxItems}&alldata={allData}" + batchIdUrl;
                    Slog.ApiCall(client, requestUrl);
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    result = await response.Content.ReadAsStringAsync();
                    CallApiEnd(sw);
                    WebApiResponseLabel.Text = $"{sw.ElapsedMilliseconds} ms";
                    if (response.IsSuccessStatusCode)
                    {
                        NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                    }
                    else
                    {
                        NoticeTextbox.Text = response.ReasonPhrase + "\n" + result;
                    }

                    Properties.Settings.Default.batchids = BatchIDTextbox.Text;
                    Properties.Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    CallApiEnd();
                    NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url, api Key and parameters";
                }
            }
        }


        private async void Button_CancelBatch(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                int batchID = 0;
                bool canConvert = int.TryParse(BatchIDTextbox.Text, out batchID);
                if (!canConvert)
                {
                    NoticeTextbox.Text = "The input BatchID is not a valid integer.";
                    return;
                }

                string result = "";

                string url = ((MainWindow) Application.Current.MainWindow).WebApiTextbox.Text;
                SetupWebClient(url, false, false);
                try
                {
                    var sw = CallApiStart();
                    var param = JsonConvert.SerializeObject(new {rejectReason = RejectReasonTextbox.Text});
                    var apiUrl = $"api/Batch/Cancel/{batchID}";
                    Slog.ApiCall(client, apiUrl, "POST", param);

                    HttpContent contentPost = new StringContent(param, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, contentPost);
                    result = await response.Content.ReadAsStringAsync();
                    
                    CallApiEnd(sw);

                    if (response.IsSuccessStatusCode)
                    {
                        NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                    }
                    else
                    {
                        NoticeTextbox.Text = response.ReasonPhrase + "\n" + result;
                    }
                }
                catch (Exception ex)
                {
                    CallApiEnd();
                    NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url, api Key and parameters";
                }
            }
        }

        private void WebApiTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //url = WebApiTextbox.Text;
            //RunAsync();

        }

        private void Button_Copy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(NoticeTextbox.Text);

        }

        private void Button_TestConnect(object sender, RoutedEventArgs e)
        {
            SetupWebClient(WebApiTextbox.Text, true, false);
        }


        private async void Button_ArchiveBatch(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                int batchID = 0;
                bool canConvert = int.TryParse(BatchIDTextbox.Text, out batchID);
                if (!canConvert)
                {
                    NoticeTextbox.Text = "The input BatchID is not a valid integer.";
                    return;
                }

                string[] ids = null;
                using(var dlg = new OlDocumentsWindow())
                {
                    var dresult = dlg.ShowDialog();
                    if ( dresult == System.Windows.Forms.DialogResult.OK )
                    {
                        ids = dlg.IntellidactIDs;
                    }
                    else
                    {
                        return;
                    }
                }

                string result = "";

                string url = ((MainWindow) Application.Current.MainWindow).WebApiTextbox.Text;
                SetupWebClient(url, false, false);
                try
                {
                    var sw = CallApiStart();
                    string param = null;
                    HttpContent contentPost = null;
                    if (ids.Length > 0)
                    {
                        param = JsonConvert.SerializeObject(ids);
                        contentPost = new StringContent(param, Encoding.UTF8, "application/json");
                    }
                    

                    var apiUrl = $"api/Batch/Archive/{batchID}";
                    Slog.ApiCall(client, apiUrl, "POST", param);
                                        
                    HttpResponseMessage response = await client.PostAsync(apiUrl, contentPost);
                    result = await response.Content.ReadAsStringAsync();
                    
                    CallApiEnd(sw);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                    }
                    else
                    {
                        NoticeTextbox.Text = response.ReasonPhrase + "\n" + result;
                    }
                }
                catch (Exception ex)
                {
                    CallApiEnd();
                    NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url, api Key and parameters";
                }
            }
        }

        private void IncludeFileReleaseDataCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            IncludeOcrCheckBox.IsChecked = false;
            IncludeOrgCheckBox.IsChecked = false;
            SavePackageCheckBox.IsChecked = false;
        }
        
        private Stopwatch CallApiStart()
        {
            WebApiResponseLabel.Text = "--";
            WebApiStatus.Text = "Waiting...";

            Properties.Settings.Default.url = WebApiTextbox.Text;
            Properties.Settings.Default.apikey = ApiKeyTextbox.Text;
            Properties.Settings.Default.maxItems = int.Parse(MaxItemsTextbox.Text);
            Properties.Settings.Default.alldata = AllDataCheckBox.IsChecked ?? true;
            Properties.Settings.Default.batch_status = StateTextbox.Text;

            Properties.Settings.Default.Save();

            return Stopwatch.StartNew();
        }

        private void CallApiEnd(Stopwatch sw)
        {
            sw.Stop();
            WebApiStatus.Text = "OK";
            WebApiResponseLabel.Text = $"{sw.ElapsedMilliseconds} ms";
        }

        private void CallApiEnd()
        {
            WebApiStatus.Text = "[Failed]";
            WebApiResponseLabel.Text = "--";
        }

        private static string BytesToStringConverted(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();


                }
            }
        }
        
    }
}
