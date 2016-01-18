using Bing;
using CarFinder.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        /// <summary>
        /// GETS all distinct years from Huge Car List Database.
        /// </summary>
        /// <returns>
        /// Returns a list of all distict years.
        /// </returns>
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
        public IHttpActionResult GetMakesByYear(string model_year)
        {
            var modelYear = new SqlParameter("@model_year", model_year);
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
        public IHttpActionResult GetModelsByYrMk(string model_year, string make)
        {
            var modelYear = new SqlParameter("@model_year", model_year);
            var Make = new SqlParameter("@make", make);
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
        public IHttpActionResult GetTrimByYrMkMod(string model_year, string make, string model_name)
        {
            var modelYear = new SqlParameter("@model_year", model_year);
            var Make = new SqlParameter("@make", make);
            var modelName = new SqlParameter("@model_name", model_name);
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
        public IHttpActionResult GetCarData(string model_year, string make, string model_name, string model_trim) 
        {
            var modelYear = new SqlParameter("@model_year", model_year ?? "");
            var Make = new SqlParameter("@make", make ?? "");
            var modelName = new SqlParameter("@model_name", model_name ?? "");
            var modelTrim = new SqlParameter("@model_trim", model_trim ?? "");
            
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
        public async Task<IHttpActionResult> GetCar(int Id)
        {
            HttpResponseMessage response;
            string content = "";
            var Car = db.Cars.Find(Id);
            var Recalls = "";
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
            Recalls = content;

            var image = new BingSearchContainer(new Uri("https://api.datamarket.azure.com/Bing/Search/v1/Image"));

            image.Credentials = new NetworkCredential("accountKey", "/8RvXOXJ4fcq5pAnNMdOtiueBjVHcWUUFAr8ZhHPJsI");
            var marketData = image.Composite(
                "image",
                Car.model_year + " " + Car.make + " " + Car.model_name + " " + Car.model_trim,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
                ).Execute();

            Image = marketData.First().Image.First().MediaUrl; 
            return Ok(new { car = Car, recalls = Recalls, image = Image });

        }


    }
}
