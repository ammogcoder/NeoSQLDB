using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace NAL
{
    /// <summary>
    /// Node Access Layer
    /// </summary>
    public class NodeLayer
    {
        public JObject InvokeMethod(string Method, params object[] Params)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    JObject joe = new JObject();
                    joe.Add(new JProperty("jsonrpc", "2.0"));
                    joe.Add(new JProperty("id", "1"));
                    joe.Add(new JProperty("method", Method));

                    // params is a collection values which the method requires..
                    if (Params != null)
                    {
                        if (Params.Length > 0)
                        {
                            JArray props = new JArray();
                            foreach (var p in Params)
                            {
                                props.Add(p);
                            }
                            joe.Add(new JProperty("params", props));
                        }
                    }
                    else
                    {
                        joe.Add(new JProperty("params", new JArray()));
                    }
                    // serialize json for the request
                    string s = JsonConvert.SerializeObject(joe);
                    byte[] byteArray = Encoding.UTF8.GetBytes(s);

                    var stringContent = new StringContent(joe.ToString());
                    //var response = client.PostAsync("http://seed2.neo.org:10332", stringContent).Result;
                    var response = client.PostAsync("http://127.0.0.1:10332", stringContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;

                        string responseObject = responseContent.ReadAsStringAsync().Result;

                        return JsonConvert.DeserializeObject<JObject>(responseObject);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {

                return null;
            }
            /*
            return responseText;

            webRequest.ContentLength = byteArray.Length;
            Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();


            WebResponse webResponse = webRequest.GetResponse();



            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("seed2.neo.org:10332");
            webRequest.ContentType = "application/json-rpc";
            webRequest.Method = "POST";

            JObject joe = new JObject();
            joe["jsonrpc"] = "1.0";
            joe["id"] = "1";
            joe["method"] = method;

            if (a_params != null)
            {
                if (a_params.Length > 0)
                {
                    JArray props = new JArray();
                    foreach (var p in a_params)
                    {
                        props.Add(p);
                    }
                    joe.Add(new JProperty("params", props));
                }
            }
            System.Net.HttpWebRequest
            string s = JsonConvert.SerializeObject(joe);
            // serialize json for the request
            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            webRequest.ContentLength = byteArray.Length;

            try
            {
                using (Stream dataStream = webRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
            }
            catch (WebException we)
            {
                throw;
            }
            WebResponse webResponse = null;
            try
            {
                using (webResponse = webRequest.GetResponse())
                {
                    using (Stream str = webResponse.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(str))
                        {
                            return JsonConvert.DeserializeObject<JObject>(sr.ReadToEnd());
                        }
                    }
                }
            }
            catch (WebException webex)
            {

                using (Stream str = webex.Response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(str))
                    {
                        var tempRet = JsonConvert.DeserializeObject<JObject>(sr.ReadToEnd());
                        return tempRet;
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }*/
        }
    }
}
