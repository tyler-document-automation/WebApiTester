namespace WebApiTester
{
    public class BatchDocumentInfo
    {
        #region Constructors
      
        public BatchDocumentInfo(string name, string file, string data, string docType, string filerData, string startingPage)
        {
            this.name = name;
            this.file = file;
            this.data = data;
            this.docType = docType;
            this.filerData = filerData;
            this.startingPage = startingPage;
        }

        #endregion
        #region Properties
        /// <summary>
        /// File Path
        /// </summary>
        public string file { get; set; }

        public string name { get; set; }
        public string data { get; set; }

        public string docType { get; set; }
        public string startingPage { get; set; }
        /// <summary>
        /// meta data that the filer used for scripting
        /// </summary>
        public string filerData { get; set; }


        #endregion
    }
}
