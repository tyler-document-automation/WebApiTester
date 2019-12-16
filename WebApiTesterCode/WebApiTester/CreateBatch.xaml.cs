using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApiTester
{
    /// <summary>
    /// Interaction logic for CreateBatch.xaml
    /// </summary>
    public partial class CreateBatch
    {
        //private static string FilePath = "";
        private static List<BatchDocumentInfo> batchDocsList;
        private static int sequence;
        private static HttpClient client;
        private static string docType;
        public static string selectedType = "--Document Type--";
        public static List<string> docTypes;
        public int uploadDocCount;

        public CreateBatch()
        {
            InitializeComponent();
            initializeProperties();
            setUpWebClient();
            BindDropDownList();
            //DataContext = new ViewModel();

        }

        public void initializeProperties()
        {
            batchDocsList = new List<BatchDocumentInfo>();
            sequence = 1;
            var handler = new HttpClientHandler();
            handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            client = new HttpClient(handler);
            DocumentsTextbox.Text = "Attached Documents: ";
        }



        private void setUpWebClient()
        {
            string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("api_key", ((MainWindow)Application.Current.MainWindow).ApiKeyTextbox.Text);
            

        }

        private async void BindDropDownList()
        {
            try
            {


                var requestUrl = $"api/Batch/DefinitionNames";
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var batchdefNames = JsonConvert.DeserializeObject<List<string>>(content);

                    BatchClassComboBox.ItemsSource = batchdefNames.Select(i => i).ToList();
                }
                else
                {
                    BatchClassComboBox.Text = "No Batch Definitions.";
                    StatusLabel.Content = $"Error in getting batch definition names from the server. \nReason : {response.ReasonPhrase}";
                    ((MainWindow)Application.Current.MainWindow).NoticeTextbox.Text = $"Error in getting batch definition names from the server. \nReason : {response.ReasonPhrase} \nPlease double check url, api Key and parameters ";
                    return;
                }


                requestUrl = $"api/Workflows/Ids";
                response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var workflows = JsonConvert.DeserializeObject<List<int>>(content);
                    WorkflowComboBox.ItemsSource = workflows.Select(i => i).ToList();
                }
                else
                {
                    WorkflowComboBox.Text = "Error in getting workflows from the server.";
                    StatusLabel.Content = $"Error in getting workflows from the server. \nReason : {response.ReasonPhrase}";

                    return;
                }

            }
            catch (Exception ex)
            {
                StatusLabel.Content = $"Error: {ex.Message}. \nPlease double check url, api Key and parameters";
            }
        }

        private void ButtonBrowseClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.tif, *.pdf)|*.tif; *.pdf|All Files|*.*",
                Multiselect = true
            };

            // Show open file dialog box
            var result = dlg.ShowDialog();
            // Process open file dialog box results
            if (result == true)
            {
                uploadDocCount = dlg.FileNames.Length;

                //if (string.IsNullOrWhiteSpace(docType)) docType = BatchClassComboBox.SelectionBoxItem.ToString();
                // Open document 
                if (FilePathCheckBox.IsChecked == true)
                {
                    foreach (var filePath in dlg.FileNames)
                    {
                        var fileName = Path.GetFileName(filePath);
                        batchDocsList.Add(new BatchDocumentInfo(fileName, filePath, null,
                            String.IsNullOrWhiteSpace(docType) ? null : docType, null));
                        DocumentsTextbox.Text += $"\n{sequence}. {docType} -> {fileName} -> filePath";
                        sequence++;
                    }
                }
                else
                {
                    foreach (var filePath in dlg.FileNames)
                    {
                        var fileName = Path.GetFileName(filePath);
                        byte[] bytes = File.ReadAllBytes(dlg.FileName);
                        var data = Convert.ToBase64String(bytes);
                        batchDocsList.Add(new BatchDocumentInfo(fileName, null, data,
                            String.IsNullOrWhiteSpace(docType) ? null : docType, null));
                        DocumentsTextbox.Text += $"\n{sequence}. {docType} -> {fileName} -> dataStream";
                        sequence++;
                    }
                }

            }
        }

        private void ButtonFilerDataClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Json Files (*.json, *.txt)|*.json;*.txt|All Files|*.*",
                Multiselect = false
            };

            // Show open file dialog box
            var result = dlg.ShowDialog();
            // Process open file dialog box results
            if (result == true)
            {
                var filePath = dlg.FileName;
                DocumentsTextbox.Text += "-> filerData ";
                string json = "";
                using (StreamReader r = new StreamReader(filePath))
                {
                    json = r.ReadToEnd();
                }

                DocumentsTextbox.Text += filePath;
                
                batchDocsList.Last().filerData = json;

            }
        }

        private void Button_CreateBatch(object sender, RoutedEventArgs e)
        {
            try
                {
                    if (BatchClassComboBox.SelectedItem == null || WorkflowComboBox.SelectedItem == null)
                    {
                        StatusLabel.Content = "Error in creating an batch. \nThe workflowId or batch class name cannot be null";
                        ((MainWindow)Application.Current.MainWindow).NoticeTextbox.Text = "Error in creating an batch. \nThe workflowId or batch class name cannot be null";

                    return;
                    }
                    int workflowId = 0;
                    int priority = 0;
                    string batchClass = BatchClassComboBox.SelectedItem.ToString();
                    if (!string.IsNullOrWhiteSpace(WorkflowComboBox.SelectedItem.ToString()))
                    {
                        bool canConvert = int.TryParse(WorkflowComboBox.SelectedItem.ToString(), out workflowId);
                        if (!canConvert)
                        {
                            StatusLabel.Content = "The input workflowID is not a valid integer.";
                            return;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(PriorityTextBox.Text))
                    {
                        bool canConvert = int.TryParse(PriorityTextBox.Text, out priority);
                        if (!canConvert)
                        {
                            StatusLabel.Content = "The input priority is not a valid integer.";
                            return;
                        }

                        if (priority < 0 || priority >= 10)
                        {
                            StatusLabel.Content = "The input priority is out of range.";
                            return;
                        }
                    }
                //var batchDocs = new List<BatchDocumentInfo>{batchdoc}.ToArray();
                    var batchDocs = batchDocsList.ToArray();
                        BatchCreateInfo batchCreateInfo = new BatchCreateInfo(
                            batchClass,
                            BatchNameTextbox.Text,
                            //priority,
                            workflowId,
                            RunIDTextbox.Text,
                            batchDocs.Length == 0 ? null : batchDocs
                            
                        );

                    StatusLabel.Content = "Creating Batch...";
                  
                    var param = JsonConvert.SerializeObject(batchCreateInfo);
                    HttpContent contentPost = new StringContent(param, Encoding.UTF8, "application/json");
                    using (new WaitCursor())
                    {
                        var response = client.PostAsync("/api/Batch/Create", contentPost).Result;
                        var result = response.Content.ReadAsStringAsync().Result;
                    

                    if (response.IsSuccessStatusCode)
                    {
                        var body = JObject.Parse(result);
                        var BatchID = body["batchId"];

                        ((MainWindow) Application.Current.MainWindow).BatchIDTextbox.Text = BatchID.ToString();
                        ((MainWindow) Application.Current.MainWindow).NoticeTextbox.Text =
                            $"Batch ID: {BatchID} successfully created for batch {batchCreateInfo.name}."
                            + Environment.NewLine
                            + JValue.Parse(result).ToString(Formatting.Indented);
                        StatusLabel.Content = $"Batch ID: {BatchID} successfully created";
                    }
                    else
                    {
                        StatusLabel.Content =
                            $"Failed to create the batch {batchCreateInfo.name}. \nSee main windows for detailed error message.";
                        ((MainWindow) Application.Current.MainWindow).BatchIDTextbox.Text = "";

                        try
                        {
                            ((MainWindow) Application.Current.MainWindow).NoticeTextbox.Text =
                                ($"Failed to create the batch {batchCreateInfo.name}. Exception: {result}");
                        }
                        catch (Exception)
                        {
                            ((MainWindow) Application.Current.MainWindow).NoticeTextbox.Text =
                                ($"Failed to create the batch {batchCreateInfo.name}. No result from the Web Api.");
                        }
                    }
                    }
            }
                catch (Exception ex)
                {
                    StatusLabel.Content = $"Error: {ex.Message}. \nPlease double check url, api Key and parameters";
                }
        }
        

        private void BatchNameTextbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClearAllDocs(object sender, RoutedEventArgs e)
        {
            DocumentsTextbox.Text = "Attached Documents: ";
            sequence = 1;
            batchDocsList = new List<BatchDocumentInfo>();
        }

        private  void BatchClassComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //DocTypeComboBox.Text = "--Document Type--";
            var batchDefName = BatchClassComboBox.SelectedItem.ToString();
            var requestUrl = $"api/Batch/DocumentDefinitionNames/{batchDefName}";
            var response = client.GetAsync(requestUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                var docDefNames = JsonConvert.DeserializeObject<List<string>>(content);
                try
                {
                    DocTypeComboBox.SelectedIndex = -1;
                }
                catch { }
                finally { 
                DocTypeComboBox.SelectedIndex = -1;
                DocTypeComboBox.SelectedValue = null;
                DocTypeComboBox.SelectedValuePath = "";
                this.DocTypeComboBox.SelectedItem = null;

                this.DocTypeComboBox.Items.Clear();
                this.DocTypeComboBox.Items.Refresh();
                //this.DocTypeComboBox.Text = docDefNames.FirstOrDefault();
                foreach (var item in docDefNames) {
                    this.DocTypeComboBox.Items.Add(item); //this.DocTypeComboBox.Items.Add(new ComboBoxItem { Content = item });
                    }
               
                //DocTypeComboBox.SelectionBoxItem = null;
                DocTypeComboBox.SelectedIndex = -1;
                DocTypeComboBox.SelectedValue = null;
                DocTypeComboBox.SelectedValuePath = "";
                this.DocTypeComboBox.SelectedItem = null;
                this.DocTypeComboBox.Text = "Document Type";
                docType = "";
                }
            }
            else
            {
                //StatusLabel.Content = $"Error in getting document type for the batch class from the server. \nReason : {response.ReasonPhrase}";
                DocTypeComboBox.Text = "No Document Type";
                return;
            }
        }

        private void DocTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            docType = DocTypeComboBox.SelectedItem.ToString();
        }

        
    }
}
