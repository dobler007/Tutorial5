using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial5.Models;
using Tutorial5.Models.DTOs;

namespace Tutorial5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnimalsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        public AnimalsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        [HttpGet]
        public IActionResult GetAnimals(string orderBy = "name")
        {
            if (!IsValidSortingParameter(orderBy))
            {
                return BadRequest("Invalid sorting parameter. Valid values are: name, description, category, area.");
            }

            var animals = new List<Animal>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                connection.Open();
                
                string orderByClause = $"ORDER BY {orderBy}";
                string sqlQuery = $"SELECT * FROM Animal {orderByClause};";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    var reader = command.ExecuteReader();

                    int idAnimalOrdinal = reader.GetOrdinal("IdAnimal");
                    int nameOrdinal = reader.GetOrdinal("Name");

                    while (reader.Read())
                    {
                        animals.Add(new Animal()
                        {
                            IdAnimal = reader.GetInt32(idAnimalOrdinal),
                            Name = reader.GetString(nameOrdinal)
                        });
                    }
                }
            }
        
            return Ok(animals);
        }

        [HttpPost]
        public IActionResult AddAnimal(AddAnimal animal)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                connection.Open();
                
                using (SqlCommand command = new SqlCommand("INSERT INTO Animal (Name) VALUES (@animalName);", connection))
                {
                    command.Parameters.AddWithValue("@animalName", animal.Name);
                    command.ExecuteNonQuery();
                }
            }

            return Created("", null);
        }

        private bool IsValidSortingParameter(string orderBy)
        {
            var validParameters = new List<string> { "name", "description", "category", "area" };
            return validParameters.Contains(orderBy.ToLower());
        }
        
    }
    
}
