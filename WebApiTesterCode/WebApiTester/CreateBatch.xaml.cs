using System;
using System.Collections.Generic;
using System.IO;
<<<<<<< HEAD
using System.Linq;
=======
>>>>>>> f0fd3c20693e9ed9f732fd6242749658cfaccad9
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
<<<<<<< HEAD
using System.Windows.Controls;
=======
>>>>>>> f0fd3c20693e9ed9f732fd6242749658cfaccad9
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
<<<<<<< HEAD
        private static List<BatchDocumentInfo> batchDocsList;
        private static int sequence;
        private static HttpClient client;
        private static string docType;
        public static string selectedType = "--Document Type--";
        public static List<string> docTypes;

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
            client = new HttpClient();
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

=======
        private static List<BatchDocumentInfo> batchDocsList = new List<BatchDocumentInfo>();
        private static int sequence = 1;
        public CreateBatch()
        {
            InitializeComponent();
            DocumentsTextbox.Text = "Attached Documents: ";
        }

>>>>>>> f0fd3c20693e9ed9f732fd6242749658cfaccad9
        private void ButtonBrowseClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
<<<<<<< HEAD
                Filter = "Image Files (*.tif, *.pdf)|*.tif; *.pdf|All Files|*.*",
                Multiselect = true
=======
                Filter = "Image Files (*.tif, *.pdf)|*.tif; *.pdf|All Files|*.*"
>>>>>>> f0fd3c20693e9ed9f732fd6242749658cfaccad9
            };

            // Show open file dialog box
            var result = dlg.ShowDialog();
            // Process open file dialog box results
            if (result == true)
            {
<<<<<<< HEAD
                
                //if (string.IsNullOrWhiteSpace(docType)) docType = BatchClassComboBox.SelectionBoxItem.ToString();
                // Open document 
                if (FilePathCheckBox.IsChecked == true)
                {
                    foreach (var filePath in dlg.FileNames)
                    {
                        var fileName = Path.GetFileName(filePath);
                        batchDocsList.Add(new BatchDocumentInfo(fileName, filePath, null,
                            String.IsNullOrWhiteSpace(docType) ? null : docType));
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
                            String.IsNullOrWhiteSpace(docType) ? null : docType));
                        DocumentsTextbox.Text += $"\n{sequence}. {docType} -> {fileName} -> dataStream";
                        sequence++;
                    }
=======
                // Open document 
                if (FilePathCheckBox.IsChecked == true)
                {
                    var filePath = dlg.FileName;
                    var fileName = Path.GetFileName(filePath);
                    batchDocsList.Add(new BatchDocumentInfo(fileName, filePath, null, String.IsNullOrWhiteSpace(DocTypeTextbox.Text) ? null : DocTypeTextbox.Text));
                    DocumentsTextbox.Text += $"\n{sequence}. {fileName}";
                    sequence++;
                }
                else
                {
                    var filePath = dlg.FileName;
                    var fileName = Path.GetFileName(filePath);
                    byte[] bytes = File.ReadAllBytes(dlg.FileName);
                    var data = Convert.ToBase64String(bytes);
                    batchDocsList.Add(new BatchDocumentInfo(fileName, null, data, String.IsNullOrWhiteSpace(DocTypeTextbox.Text) ? null : DocTypeTextbox.Text));
                    DocumentsTextbox.Text += $"\n{sequence}. {fileName}";
                    sequence++;
>>>>>>> f0fd3c20693e9ed9f732fd6242749658cfaccad9
                }

            }
        }

        private void Button_CreateBatch(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
            try
                {
                    if (BatchClassComboBox.SelectedItem == null || WorkflowComboBox.SelectedItem == null)
                    {
                        StatusLabel.Content = "Error in creating an batch. \nThe workflowId or batch class name cannot be null";
                        ((MainWindow)Application.Current.MainWindow).NoticeTextbox.Text = "Error in creating an batch. \nThe workflowId or batch class name cannot be null";

                    return;
                    }
                    int workflowId = 0;
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
                    //var batchDocs = new List<BatchDocumentInfo>{batchdoc}.ToArray();
                    var batchDocs = batchDocsList.ToArray();
                    BatchCreateInfo batchCreateInfo = new BatchCreateInfo(
                        batchClass,
                        BatchNameTextbox.Text,
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
        
=======
            HttpClient client = new HttpClient();
            try
            {
                //var batchdoc = new BatchDocumentInfo((FilePathCheckBox.IsChecked != null && FilePathCheckBox.IsChecked == true) ? "" : Path.GetFileName(FilePath), (FilePathCheckBox.IsChecked != null && FilePathCheckBox.IsChecked == true) ? FilePath : "", (FilePathCheckBox.IsChecked != null && FilePathCheckBox.IsChecked == true) ? "" : FilePathTextbox.Text, DocTypeTextbox.Text);
                int workflowId = 0;

                if (!string.IsNullOrWhiteSpace(WorkflowTextbox.Text))
                {
                    bool canConvert = int.TryParse(WorkflowTextbox.Text, out workflowId);
                    if (!canConvert)
                    {
                        StatusLabel.Content = "The input workflowID is not a valid integer.";
                        return;
                    }
                }
                //var batchDocs = new List<BatchDocumentInfo>{batchdoc}.ToArray();

                var batchDocs = batchDocsList.ToArray();
                string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
                BatchCreateInfo batchCreateInfo = new BatchCreateInfo(
                    ClassNameTextbox.Text,
                    BatchNameTextbox.Text,
                    int.Parse(WorkflowTextbox.Text),
                    RunIDTextbox.Text,
                    batchDocs
                );
                StatusLabel.Content = "Creating Batch...";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
           
                var param = JsonConvert.SerializeObject(batchCreateInfo);
                HttpContent contentPost = new StringContent(param, Encoding.UTF8, "application/json");

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
                    StatusLabel.Content = $"Failed to create the batch {batchCreateInfo.name}. \nSee main windows for detailed error message.";
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
            catch (Exception ex)
            {
                StatusLabel.Content = $"Error: {ex.Message}. \nPlease double check url and parameters";
            }
        }
>>>>>>> f0fd3c20693e9ed9f732fd6242749658cfaccad9

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
<<<<<<< HEAD

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

        //private void ButtonLoadDocumentType(object sender, RoutedEventArgs e)
        //{
           
        //    ////DocTypeComboBox.Text = "--Document Type--";
        //    //var batchDefName = BatchClassComboBox.SelectedItem.ToString();
        //    //var requestUrl = $"api/Batch/DocumentDefinitionNames/{batchDefName}";
        //    //var response = client.GetAsync(requestUrl).Result;

        //    //if (response.IsSuccessStatusCode)
        //    //{
        //    //    var content = response.Content.ReadAsStringAsync().Result;
        //    //    var docDefNames = JsonConvert.DeserializeObject<List<string>>(content);

        //    //    this.DocTypeComboBox.ItemsSource = docDefNames.Select(i => i).ToList();
        //    //}
        //    //else
        //    //{
        //    //    //StatusLabel.Content = $"Error in getting document type for the batch class from the server. \nReason : {response.ReasonPhrase}";
        //    //    DocTypeComboBox.Text = "No Document Type";
        //    //    return;
        //    //}
            
        //}
=======
>>>>>>> f0fd3c20693e9ed9f732fd6242749658cfaccad9
    }
}
