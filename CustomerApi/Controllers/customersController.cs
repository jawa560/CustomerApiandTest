using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using CustomerApi.Models;

namespace CustomerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private static List<Customer> customers = new List<Customer>();

        /// <summary>
        /// 查詢所有客戶資料:
        /// </summary>
        /// <returns>目前客戶清單:</returns>
        /// <response code="200">顯示所有客戶.</response>
        /// <response code="401">沒有權限可開啟本頁.</response>
        /// <response code="404">尚無資料.</response>
        /// 
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(IEnumerable<Customer>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public ActionResult<IEnumerable<Customer>> Get()
        {
            return customers;
        }

        /// <summary>
        /// 查詢客戶資料:
        /// </summary>
        /// <param name="id">要查詢的客戶ID:</param>
        /// <returns>該ID的客戶資料是:</returns>
        /// <response code="200">查詢到客戶資料.</response>
        /// <response code="401">沒有權限可開啟本頁.</response>
        /// <response code="404">尚無資料.</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(Customer), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public ActionResult<Customer> Get(int id)
        {
            var customer = customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            return customer;
        }

        /// <summary>
        /// 建立一位新客戶.
        /// </summary>
        /// <param name="customer">要建立的客戶資料內容.</param>
        /// <returns>已建立的客戶資料內容:</returns>
        /// <response code="201">新客戶添加成功.</response>
        /// <response code="400">無效的輸入值.</response>
        /// <response code="401">沒有權限可開啟本頁.</response>
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(Customer), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public ActionResult<Customer> Post([FromBody] Customer customer)
        {
            customers.Add(customer);
            customer.Id = customers.Count + 1;
            return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
        }

        /// <summary>
        /// 更改客戶資料:
        /// </summary>
        /// <param name="id">要更改的客戶 ID:</param>
        /// <param name="customer">要更改的客戶資料:</param>
        /// <returns>No content.</returns>
        /// <response code="204">客戶資料更改成功.</response>
        /// <response code="400">無效的輸入值.</response>
        /// <response code="401">沒有權限可開啟本頁.</response>
        /// <response code="404">無此ID資料.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult Put(int id, [FromBody] Customer customer)
        {
            var existingCustomer = customers.FirstOrDefault(c => c.Id == id);
            if (existingCustomer == null)
            {
                return NotFound();
            }
            existingCustomer.Name = customer.Name;
            existingCustomer.Birthday = customer.Birthday;
            existingCustomer.Gender = customer.Gender;
            existingCustomer.Address = customer.Address;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Note1 = customer.Note1;
            existingCustomer.Note2 = customer.Note2;

            return NoContent();
        }

        /// <summary>
        /// 刪除客戶資料.
        /// </summary>
        /// <param name="id">要刪除的客戶 ID:</param>
        /// <returns>No content.</returns>
        /// <response code="204">客戶資料刪除成功.</response>
        /// <response code="401">沒有權限可開啟本頁.</response>
        /// <response code="404">尚無資料.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult Delete(int id)
        {
            var customer = customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            customers.Remove(customer);
            return NoContent();
        }
    }
}
