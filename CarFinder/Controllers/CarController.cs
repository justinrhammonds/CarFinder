using CarFinder.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CarFinder.Controllers
{
    public class CarController : ApiController
    {
        //this is the link back to your database content
        private ApplicationDbContext db = new ApplicationDbContext();

        //you need one of these for every stored procedure
        //GET UNIQUE YEARS
        public IHttpActionResult GetUniqueYears()
        {
            var retval = db.Database.SqlQuery<string>( //creates a raw SQL query for the Database 'Car' w/ EXEC statement enclosed in ""
                "EXEC GetUniqueYears").ToList();

            return Ok(retval);
        }

        //you need one of these for every stored procedure
        //GET MAKES BY YEARS
        public IHttpActionResult GetMakesByYear(string model_year)
        {
            var modelYear = new SqlParameter("@model_year", model_year);
            var retval = db.Database.SqlQuery<string>( //creates a raw SQL query for the Database 'Car' w/ EXEC statement enclosed in ""
                "EXEC GetMakesByYear @model_year",
                modelYear).ToList();

            return Ok(retval);
        }

        //you need one of these for every stored procedure
        //GET MODELS BY MAKE AND YEAR
        public IHttpActionResult GetModelsByYrMk(string model_year, string make)
        {
            var modelYear = new SqlParameter("@model_year", model_year);
            var carMake = new SqlParameter("@make", make);
            var retval = db.Database.SqlQuery<string>( //creates a raw SQL query for the Database 'Car' w/ EXEC statement enclosed in ""
                "EXEC GetModelsByYrMk @model_year, @make", modelYear, carMake).ToList();

            return Ok(retval);
        }

        //you need one of these for every stored procedure
        //GET CAR DATA BY YEAR
        public IHttpActionResult GetCarDataByYr(string cars_year) 
        {
            var carsByYear = new SqlParameter("@model_year", cars_year);
            var retval = db.Database.SqlQuery<Car>( //creates a raw SQL query for the Database w/ EXEC statement enclosed in ""
                "EXEC GetCarDataByYr @model_year",
                carsByYear).ToList();

            return Ok(retval);
        }

    }
}
