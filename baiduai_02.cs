using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class baiduai_02 : MonoBehaviour
{
    public TextMeshPro meshPro;
    // 短语识别器
    private PhraseRecognizer m_PhraseRecognizer;
    // 关键字
    public string[] keywords = { "你好", "再见", "回头见", "猪呀", "你是白痴", "小笨蛋", "hi", "见到你很高兴", "你好聪明呀", "你真厉害", "你好棒", "谢谢你", "非常感谢你", "很感谢" };
    // 可信度
    public ConfidenceLevel m_confidenceLevel = ConfidenceLevel.Medium;
    // Start is called before the first frame update
    void Start()
    {
        if (m_PhraseRecognizer == null)
        {
            //创建一个识别器
            m_PhraseRecognizer = new KeywordRecognizer(keywords, m_confidenceLevel);
            //通过注册监听的方法
            m_PhraseRecognizer.OnPhraseRecognized += M_PhraseRecognizer_OnPhraseRecognized;
            //开启识别器
            m_PhraseRecognizer.Start();

            Debug.Log("创建识别器成功");
        }
    }

    private PhraseRecognizedEventArgs temp;
    private void M_PhraseRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        temp = args;
        SpeechRecognition();
        print(args.text);
    }

    private void OnDestroy()
    {
        //判断场景中是否存在语音识别器，如果有，释放
        if (m_PhraseRecognizer != null)
        {
            //用完应该释放，否则会带来额外的开销
            m_PhraseRecognizer.Dispose();
        }

    }

    void SpeechRecognition()
    {
        //获取认证
        // String accessToken = AccessToken.getAccessToken();
        //调用接口chart接口
        Utterance.unit_utterance(temp);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static class AccessToken
    {
        // 调用getAccessToken()获取的 access_token建议根据expires_in 时间 设置缓存
        // 返回token示例
        public static String TOKEN = "24.adda70c11b9786206253ddb70affdc46.2592000.1493524354.282335-1234567";
        // 百度云中开通对应服务应用的 API Key 建议开通应用的时候多选服务
        private static String clientId = "NCVVQHBCK4eu0z8GQhkBj8qr";
        // 百度云中开通对应服务应用的 Secret Key
        private static String clientSecret = "FRKd6Kq0NfkcW0c1sLMsWGhG0BFTjcDS";
        public static String getAccessToken()
        {
            String authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new HttpClient();
            List<KeyValuePair<String, String>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", clientId));
            paraList.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            String result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
            Debug.Log(result);
            //TOKEN = result;
            return result;
        }
    }
    public class Utterance
    {
        // unit对话接口
        public static string unit_utterance(PhraseRecognizedEventArgs args)
        {
            TextMeshPro meshPro = GameObject.Find("Answer").GetComponent<TextMeshPro>();
            JObject jo = (JObject)JsonConvert.DeserializeObject(AccessToken.getAccessToken());
            string token = jo["access_token"].ToString();
            //string message = jo["Message"].ToString();
            // string token = AccessToken.getAccessToken();//token已经获取
            string host = "https://aip.baidubce.com/rpc/2.0/unit/service/v3/chat?access_token=" + token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.ContentType = "application/json";
            request.KeepAlive = true;
            //改成自己的！
            string wen = args.text;
            string str = "{\"version\":\"3.0\",\"service_id\":\"S88678\",\"session_id\":\"\",\"log_id\":\"1335917\",\"request\":{\"terminal_id\":\"88888\",\"query\":\"" + wen + "\"}}"; // json格式  S10000 需要自己的！
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            Console.WriteLine("对话接口返回:");
            // Debug.Log("________json________result______________________"+result);//看一下json格式
            Console.WriteLine(result);
            //解析json
            /*
            1.Json字符串嵌套格式解析
            string jsonText = "{\"beijing\":{\"zone\":\"海淀\",\"zone_en\":\"haidian\"}}";
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonText);
            string zone = jo["beijing"]["zone"].ToString();
            string zone_en = jo["beijing"]["zone_en"].ToString();       
            2.Json字符串数组格式解析
            string jsonArrayText = "[{'a':'a1','b':'b1'},{'a':'a2','b':'b2'}]"; //"[{'a':'a1','b':'b1'}]即使只有一个元素，也需要加上[]
            string jsonArrayText = "[{\"a\":\"a1\",\"b\":\"b1\"},{\"a\":\"a2\",\"b\":\"b2\"}]";  //上面写法和此写法效果一样
            JArray jArray = (JArray)JsonConvert.DeserializeObject(jsonArrayText);//jsonArrayText必须是带[]数组格式字符串
            string str = jArray[0]["a"].ToString();
             */
            //Json字符串嵌套格式解析
            Debug.Log(result);
            JObject jresult = (JObject)JsonConvert.DeserializeObject(result);
            string sjresult = jresult["result"]["responses"].ToString();
            Debug.Log(sjresult);

            //Json字符串数组格式解析
            JArray jArray = (JArray)JsonConvert.DeserializeObject(sjresult);
            string jactions = jArray[0]["actions"].ToString();
            Debug.Log(jactions);

            //Json字符串数组格式解析
            JArray jArray1 = (JArray)JsonConvert.DeserializeObject(jactions);
            string jactions1 = jArray1[0]["say"].ToString();
            Debug.Log(jactions1);
            //咏鹅，唐，骆宾王。鹅鹅鹅，曲项向天歌。白毛浮绿水，红掌拨清波。
            ///另外一种解析方式：///
            /*            
                JObject resultObj = (JObject)JsonConvert.DeserializeObject(result);
                string actions = resultObj["result"]["responses"].ToString();

                JArray sayObj = (JArray)JsonConvert.DeserializeObject(actions);
                string responses = sayObj[0].ToString();
            //区别处：
                JObject actionsObj = (JObject)JsonConvert.DeserializeObject(responses);
                string sayobj = actionsObj.GetValue("actions")[0].ToString();

                JObject ac = (JObject)JsonConvert.DeserializeObject(sayobj);
                string acc = ac.GetValue("say").ToString();
            */
            meshPro.text = jactions1;
            return result;
        }
    }
}

