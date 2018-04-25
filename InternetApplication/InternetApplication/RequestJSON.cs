using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace InternetApplication
{
    class RequestJSON
    {
        private static JsonObject _source;
        private RequestJSON() { }
        public static string GetJSON (string content)
        {
            return @"{
            'reqType':0,
            'perception': {
                'inputText': {
                    'text': '" + content + @"'
                }
            },
            'userInfo': {
                'apiKey': 'bc475ba1bf144ad7bbfac404b5e6a7ca',
                'userId': '117983003901'
            }
            }";
        }

        public static void SetSource (string content)
        {
            _source = JsonObject.Parse(content);
        }

        public static string GetText()
        {
            foreach(var item in _source["results"].GetArray())
            {
                if (item.GetObject()["resultType"].GetString() == "text")
                    return item.GetObject()["values"].GetObject()["text"].GetString();
            }
            return "哎呀，好像出错了！";
        }

        public static string GetURL()
        {
            foreach (var item in _source["results"].GetArray())
            {
                if (item.GetObject()["resultType"].GetString() == "url")
                    return item.GetObject()["values"].GetObject()["url"].GetString();
            }
            return "";
        }
    }
}
