using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using TestLuis.Models;
using System.Data.SqlClient;

namespace TestLuis.Controllers.api 
{
    public class LuisController : ApiController
    {
        public string SelectDB(string Question)
        {
            string[] question = { "", "" };
            question = TestLuis(Question).Result;

            SqlConnection conn = new SqlConnection("data source = localhost; initial catalog = test; user id = root; password = 123456");
            conn.Open();
            SqlCommand cmd = new SqlCommand(
                "SELECT Answer.ans" +
                "FROM  Question , Answer " +
                "WHERE Question.id = Answer.id" +
                "AND  Question.intent =" + "'" + question[0] + "'" +
                "AND  Question.entity =" + "'" + question[1] + "'", conn);

            SqlDataReader dr = cmd.ExecuteReader();

            try
            {
                while (dr.Read())
                {
                    return dr[0].ToString();
                }
            }
            finally
            {
                dr.Close();
            }

            return "";

        }



        public async Task<string[]> TestLuis(string Question)
        {

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // This app ID is for a public sample app that recognizes requests to turn on and turn off lights
            var luisAppId = "62cf89a0-8a75-4c70-991d-1c74d3d98809";
            var endpointKey = "56a82676b9854b3b86f76dae6db1c17d";

            // The request header contains your subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", endpointKey);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["q"] = Question;

            // These optional request parameters are set to their default values
            queryString["timezoneOffset"] = "0";
            queryString["verbose"] = "false";
            queryString["spellCheck"] = "false";
            queryString["staging"] = "false";

            var endpointUri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + luisAppId + "?" + queryString;
            var response = await client.GetAsync(endpointUri);
            var strResponseContent = await response.Content.ReadAsStringAsync();

            RootObject RootObject = JsonConvert.DeserializeObject<RootObject>(strResponseContent);

            string[] luis = new string[2];
            luis[0] = RootObject.topScoringIntent.intent;
            luis[1] = RootObject.entities[0].type;

            return luis;
         
        }

    }
}
