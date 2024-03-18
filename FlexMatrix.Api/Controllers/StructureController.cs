using FlexMatrix.Api.Data.DataBase;
using FlexMatrix.Api.Data.Models;
using FlexMatrix.Api.Data.Services;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork = FlexMatrix.Api.Attributes.UnitOfWorkAttribute;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlexMatrix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StructureController : ControllerBase
    {
        private readonly IStructureService _structureService;


        public StructureController(IStructureService structureService)
        {
            _structureService = structureService;
        }


        [UnitOfWork]
        [HttpPost(Name = "CreateNewTableStructure")]
        public async Task<IActionResult> CreateNewTableStructure(TableStructureDto tableStructure)
        {
            var result = await _structureService.CreateTableStructure(tableStructure);
            if (result)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, "Internal server error: " + "empty");
            }
        }







        //// GET: api/<StructureController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<StructureController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<StructureController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<StructureController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<StructureController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
