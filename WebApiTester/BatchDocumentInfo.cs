namespace WebApiTester
{
    public class BatchDocumentInfo
    {
        #region Constructors
      
        public BatchDocumentInfo(string name, string file, string data, string docType, string filerData, string startingPage, string externalDocId, string userData)
        {
            this.name = name;
            this.file = file;
            this.data = data;
            this.docType = docType;
            this.filerData = filerData;
            this.startingPage = startingPage;
            this.externalId = externalDocId;
            this.userData = userData;
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
        /// <summary>
        /// User generated document id
        /// </summary>
        public string externalId { get; set; }
        public string userData { get; set; }

        #endregion
    }
}
