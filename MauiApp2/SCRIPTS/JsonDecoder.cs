using System;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace MauiApp2.SCRIPTS
{
    public static class JsonDecoder
    {
        public static string DecodeConcatenatedJSON(string concatenatedChunks)
        {
            //Debug.WriteLine("decodeJSON initialized with concatenatedChunks: " + concatenatedChunks);

            var fullMessage = new StringBuilder();

            try
            {
                // Split the concatenatedChunks by line
                var chunks = concatenatedChunks.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var chunk in chunks)
                {
                    try
                    {
                        var json = JObject.Parse(chunk);
                        var imessage = json["message"]?.ToString();
                        var start_of_message = json["start_of_message"]?.ToString();

                        if (start_of_message != null)
                        {
                            // Handle start_of_message if needed
                        }

                        if (imessage != null)
                        {
                            fullMessage.Append(imessage);
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        //Debug.WriteLine("Json parser exception: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
               // Debug.WriteLine("Error while processing concatenatedChunks: " + ex.Message);
            }

            //Debug.WriteLine("decodeJSON: " + fullMessage.ToString());

            return fullMessage.ToString();
        }
    }
}
