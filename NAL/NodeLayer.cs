using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;

namespace NAL
{
    /// <summary>
    /// Node Access Layer
    /// </summary>
    public class NodeLayer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public JObject Invoke(string method, List<string> nodes, params object[] Params)
        {
            JObject joe = null;
            while (joe == null)
            {
                foreach (var node in nodes)
                {
                    joe = InvokeMethod(method, node, Params);
                    if (joe != null)
                    {
                        break;
                    }
                }
            }
            return joe;
        }

        //TODO: method for getting best node from list (= node with highest block count)
        private JObject InvokeMethod(string Method, string Node, params object[] Params)
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
                    var response = client.PostAsync(Node, stringContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;

                        string responseObject = responseContent.ReadAsStringAsync().Result;

                        JObject result = JsonConvert.DeserializeObject<JObject>(responseObject);
                        if (result["error"] == null)
                        {
                            return result;
                        }
                        else if (result["error"]["message"].ToString() == "Unknown block")
                        {
                            return result;
                        }
                        else
                        {
                            return null;
                        }

                        //return JsonConvert.DeserializeObject<JObject>(responseObject);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                using (TextWriter errorWriter = Console.Error)
                {
                    errorWriter.WriteLine(e.Message);
                }
                return null;
            }
        }
    }
}
