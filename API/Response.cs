namespace DevBin.API {
    public class Response {
        public int Code { get; set; }
        public string Message { get; set; }
        public bool Ok { get; set; }

        /// <summary>
        /// Represents an API request error
        /// </summary>
        /// <param name="code">Code provided by the API</param>
        /// <param name="message">Message provided by the API</param>
        /// <param name="ok">Whether the response is successful or not</param>
        public Response(int code, string message, bool ok = true) {
            Code = code;
            Message = message;
            Ok = ok;
        }
    }
}