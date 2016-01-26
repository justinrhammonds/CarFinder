using Bing;
using CarFinder.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CarFinder.Controllers
{
    /// <summary>
    /// Provides functionality for the type 'Car'.
    /// </summary>
    /// <remarks>
    /// Contains methods that retrieve specific data about cars from the Huge Car List Database, 
    /// recall information from the National Highway Traffic Safety Administration, and an 
    /// associated image via a third party Search API from Bing
    /// </remarks>
    public class CarController : ApiController
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        public class Selected
        {
            public string year { get; set; }
            public string make { get; set; }
            public string model { get; set; }
            public string trim { get; set; }
        }

        public class IdParam
        {
            public int id { get; set; }
        }



        /// <summary>
        /// GETS all distinct years from Huge Car List Database.
        /// </summary>
        /// <returns>
        /// Returns a list of all distict years.
        /// </returns>
        [HttpPost]
        public IHttpActionResult GetUniqueYears()
        {
            var retval = db.Database.SqlQuery<string>(
                "EXEC GetUniqueYears").ToList();

            return Ok(retval);
        }

        /// <summary>
        /// GETS all distinct car makes from the Huge Car List Database.
        /// </summary>
        /// <remarks>
        /// Requires the input parameter <paramref name="model_year"/>
        /// </remarks>
        /// <param name="model_year">The Year</param>
        /// <returns>
        /// Returns a list of all distinct car makes.
        /// </returns>
        [HttpPost]
        public IHttpActionResult GetMakesByYear(Selected selected)
        {
            var modelYear = new SqlParameter("@model_year", selected.year);
            var retval = db.Database.SqlQuery<string>(
                "EXEC GetMakesByYear @model_year",
                modelYear).ToList();

            return Ok(retval);
        }


        /// <summary>
        /// GETS all distinct car models from the Huge Car List Database.
        /// </summary>
        /// <remarks>
        /// Requires the input parameters <paramref name="model_year"/>, and <paramref name="make"/>
        /// </remarks>
        /// <param name="model_year">The Year</param>
        /// <param name="make">The Make</param>
        /// <returns>
        /// Returns a list of all distinct car models.
        /// </returns>
        [HttpPost]
        public IHttpActionResult GetModelsByYrMk(Selected selected)
        {
            var modelYear = new SqlParameter("@model_year", selected.year);
            var Make = new SqlParameter("@make", selected.make);
            var retval = db.Database.SqlQuery<string>(
                "EXEC GetModelsByYrMk @model_year, @make", modelYear, Make).ToList();

            return Ok(retval);
        }


        /// <summary>
        /// GETS all distinct car trims from the Huge Car List Database.
        /// </summary>
        /// <remarks>
        /// Requires the input parameters <paramref name="model_year"/>, <paramref name="make"/>, and <paramref name="model_name"/>
        /// </remarks>
        /// <param name="model_year">The Year</param>
        /// <param name="make">The Make</param>
        /// <param name="model_name">The Model</param>
        /// <returns>
        /// Returns a list of all distinct car models.
        /// </returns>
        [HttpPost]
        public IHttpActionResult GetTrimByYrMkMod(Selected selected)
        {
            var modelYear = new SqlParameter("@model_year", selected.year);
            var Make = new SqlParameter("@make", selected.make);
            var modelName = new SqlParameter("@model_name", selected.model);
            var retval = db.Database.SqlQuery<string>(
                "EXEC GetTrimByYrMkMod @model_year, @make, @model_name", modelYear, Make, modelName).ToList();

            return Ok(retval);
        }


        /// <summary>
        /// GETS all car information from the Huge Car List Database.
        /// </summary>
        /// <remarks>
        /// As an option, the method accepts the following input parameters as filters: 
        /// <paramref name="model_year"/>, <paramref name="make"/>, <paramref name="model_name"/>, and <paramref name="model_trim"/>
        /// </remarks>
        /// <param name="model_year">The Year</param>
        /// <param name="make">The Make</param>
        /// <param name="model_name">The Model</param>
        /// <param name="model_trim">The Trim</param>
        /// <returns>
        /// Returns a list of all car data from the specified input parameters.
        /// </returns>
        [HttpPost]
        public IHttpActionResult GetCarData(Selected selected)
        {
            var modelYear = new SqlParameter("@model_year", selected.year ?? "");
            var Make = new SqlParameter("@make", selected.make ?? "");
            var modelName = new SqlParameter("@model_name", selected.model ?? "");
            var modelTrim = new SqlParameter("@model_trim", selected.trim ?? "");

            var retval = db.Database.SqlQuery<Car>(
                "EXEC GetCarData @model_year, @make, @model_name, @model_trim", modelYear, Make, modelName, modelTrim).ToList();

            return Ok(retval);
        }

        /// <summary>
        /// Retieves info from third party APIs: National Highway Traffic Safety Administration and Bing Search    
        /// </summary>
        /// <param name="Id">The Car Record's ID</param>
        /// <returns>
        /// Returns any existing recall information (NHTSA) and an associated image for a specified car.
        /// </returns>
        [HttpPost]
        public async Task<IHttpActionResult> GetCar(IdParam id) //(Selected selected)
        {
            HttpResponseMessage response;
            string content = "";
            var Car = db.Cars.Find(id.id); //(Selected selected) passes in model, make, 
            dynamic Recalls = "";
            var Image = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://www.nhtsa.gov/");
                try
                {
                    response = await client.GetAsync("webapi/api/Recalls/vehicle/modelyear/" + Car.model_year +
                                                                                    "/make/" + Car.make +
                                                                                    "/model/" + Car.model_name + "?format=json");
                    content = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                }
            }
            Recalls = JsonConvert.DeserializeObject(content);

            var image = new BingSearchContainer(new Uri("https://api.datamarket.azure.com/Bing/Search/v1/Composite"));

            image.Credentials = new NetworkCredential("accountKey", "/8RvXOXJ4fcq5pAnNMdOtiueBjVHcWUUFAr8ZhHPJsI");
            var marketData = image.Composite(
                "image",
                Car.model_year + " " + Car.make + " " + Car.model_name + " " + Car.model_trim + " " + "NOT ebay",
                null,
                null,
                null,
                "Moderate",
                null,
                null,
                null,
                "Size:Large+Style:Photo",
                null,
                null,
                null,
                null,
                null
                ).Execute();

            var Images = marketData.FirstOrDefault()?.Image;
            foreach (var Img in Images)
            {
                
                if (UrlCtrl.IsUrl(Img.MediaUrl))
                {
                    Image = Img.MediaUrl;
                    break;
                } else
                {
                    continue;
                }

            }

            if (string.IsNullOrWhiteSpace(Image))
            {
                Image = "../img/car404.jpg";
            }

            return Ok(new { car = Car, recalls = Recalls, image = Image });

        }

    }

    public static class UrlCtrl 
    {
        public static bool IsUrl(string path)
        {
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(path);
            request.Method = "HEAD";
            bool result = true;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                /* A WebException will be thrown if the status of the response is not `200 OK` */
                result = false;
            }
            finally
            {
                // Don't forget to close your response.
                if (response != null)
                {
                    response.Close();
                }

            }

            return result;

        }

    }

}
