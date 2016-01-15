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
    public class CarController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// This method references a stored procedure that gets all distinct years from the Huge Car List Database.
        /// </summary>
        /// <returns>
        /// Returns all distict years in descending order beginning with the most recent year.
        /// </returns>
        public IHttpActionResult GetUniqueYears()
        {
            var retval = db.Database.SqlQuery<string>(
                "EXEC GetUniqueYears").ToList();

            return Ok(retval);
        }

        /// <summary>
        /// This method references a stored procedure that gets all distinct car makes from a specific parameter 'model_year' from the Huge Car List Database.
        /// </summary>
        /// <param name="model_year">The Year</param>
        /// <returns>
        /// Returns all distinct car makes from specified parameter 'model_year'in ascending alphabetical order.
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
        /// This method references a stored procedure that gets all distinct car models from specific parameters 'model_year' and 'make' from the Huge Car List Database.
        /// </summary>
        /// <param name="model_year">The Year</param>
        /// <param name="make">The Make</param>
        /// <returns>
        /// Returns all distinct car models from specified parameters 'model_year' and 'make' in ascending alphabetical order.
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
        /// This method references a stored procedure that gets all distinct car trim from specific parameters 'model_year', 'make', and 'model_name' from the Huge Car List Database.
        /// </summary>
        /// <param name="model_year">The Year</param>
        /// <param name="make">The Make</param>
        /// <param name="model_name">The Model</param>
        /// <returns>
        /// Returns all distinct car models from specified parameters 'model_year', 'make', and 'model_name' in ascending alphabetical order.
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
        /// This method references a stored procedure that gets all car information from the Huge Car List Database. Optionally, it accepts values for parameters in the following order: 'model_year', 'make', and 'model_name', 'model_trim'.
        /// </summary>
        /// <param name="model_year">The Year</param>
        /// <param name="make">The Make</param>
        /// <param name="model_name">The Model</param>
        /// <param name="model_trim">The Trim</param>
        /// <returns>
        /// Returns all car information and optionally filters records by specified parameters 'model_year', 'make', model_name', and 'model_trim' sorted in alphabetical order by make,model_name, and model_trim.
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
        /// This method references the third party API's from National Highway Traffic Safety Administration and Bing     
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

            var image = new BingSearchContainer(new Uri("https://api.datamarket.azure.com/Bing/search/"));

            image.Credentials = new NetworkCredential("accountKey", "8RvXOXJ4fcq5pAnNMdOtiueBjVHcWUUFAr8ZhHPJsI");
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
