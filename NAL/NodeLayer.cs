using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace NAL
{
    /// <summary>
    /// Node Access Layer
    /// </summary>
    public class NodeLayer
    {
        public JObject InvokeMethod(string Method, string Node, params object[] Params)
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
            catch (Exception)
            {

                return null;
            }
        }
    }
}
