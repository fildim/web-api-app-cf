using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiApp.Data;
using WebApiApp.DTO;

namespace WebApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly EshopDbContext _context;
        private readonly IMapper _mapper;

        private List<Error> errorArray = new();

        public CustomersController(EshopDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _context.Customers.ToListAsync();
            var customersDto = _mapper.Map<IEnumerable<CustomerReadOnlyDTO>>(customers);

            return Ok(customersDto);            
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerReadOnlyDTO>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            
            if (_context.Customers == null || customer is null)
            {
              return NotFound();
            }

            var customerDto = _mapper.Map<CustomerReadOnlyDTO>(customer);

            return customerDto;
        }

        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, CustomerUpdateDTO customerUpdateDto)
        {
            if (id != customerUpdateDto.Id)
            {
                return BadRequest();
            }

            var customer = _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            var updatedCustomer = _mapper.Map<CustomerUpdateDTO>(customer);
            var customerToUpdate = _mapper.Map<Customer>(updatedCustomer);

            customerToUpdate.PhoneNo = customerUpdateDto.PhoneNo;
            customerToUpdate.Address = customerUpdateDto.Address;

            try
            {
                _context.Attach(customerToUpdate);
                _context.Entry(customerToUpdate).State = EntityState.Modified;
            
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Problem("Internal Server Error");
            }

            return NoContent();
        }

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CustomerReadOnlyDTO>> PostCustomer(CustomerCreateDTO customerCreateDto)
        {
            if (_context.Customers == null)
            {
                return Problem("Entity set 'EshopDbContext.Customers'  is null.");
            }

            try
            {
                var customer = _mapper.Map<Customer>(customerCreateDto);

                _context.Customers.Add(customer);

                await _context.SaveChangesAsync();

                var dto = _mapper.Map<CustomerReadOnlyDTO>(customer);

                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, dto);

            } catch (Exception)
            {
                return BadRequest();
            }                       
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (_context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("GetCustomersByVat/{vat}")]
        public async Task<ActionResult<CustomerReadOnlyDTO>> GetCustomerByVat(string? vat)
        {
            var customer= await _context.Customers.FirstOrDefaultAsync(e => e.VatRegNo == vat);
            if (customer == null) return NotFound();

            var customerDto = _mapper.Map<CustomerReadOnlyDTO>(customer);

            return Ok(customerDto);
        }


        [HttpGet("ValidateVat/{vat}")]
        public IActionResult ValidateVat(string vat)
        {
            var isValid = CheckVatRegNo(vat);

            if (!isValid) return BadRequest($"{errorArray[0].Code} {errorArray[0].Message} {errorArray[0].Field}");

            return Ok(isValid);
        }


        private bool CheckVatRegNo(string vatRegNo)
        {
            bool validFormat = true;

            if (vatRegNo != null && vatRegNo != "")
            {
                var pattern = @"^[0-9]{9}$";
                Regex rg = new Regex(pattern);

                if (rg.IsMatch(vatRegNo))
                {
                    var mySum = 0;
                    
                    mySum = Int32.Parse(vatRegNo.Substring(0, 1)) * 256;
                    mySum = mySum + Int32.Parse(vatRegNo.Substring(1, 1)) * 128;
                    mySum = mySum + Int32.Parse(vatRegNo.Substring(2, 1)) * 64;
                    mySum = mySum + Int32.Parse(vatRegNo.Substring(3, 1)) * 32;
                    mySum = mySum + Int32.Parse(vatRegNo.Substring(4, 1)) * 16;
                    mySum = mySum + Int32.Parse(vatRegNo.Substring(5, 1)) * 8;
                    mySum = mySum + Int32.Parse(vatRegNo.Substring(6, 1)) * 4;
                    mySum = mySum + Int32.Parse(vatRegNo.Substring(7, 1)) * 2;

                    var mymod = mySum % 11;

                    if (!(((mymod == 10) && (Int32.Parse(vatRegNo.Substring(8,1)) == 0)) || ((mymod != 10) && (Int32.Parse(vatRegNo.Substring(8, 1)) == mymod))))
                    {
                        var errorRec = new Error("IncorrectVatRegNo", "Please fill in a correct Vat Reg. No.", "VatRegistrationNo");
                        errorArray.Add(errorRec);
                        validFormat = false;
                    }

                }
                else
                {
                    var errorRec = new Error("IncorrectVatRegNo", "Please fill in a correct Vat Reg. No.", "VatRegistrationNo");
                    errorArray.Add(errorRec);
                    validFormat = false;
                }

            }
            else
            {
                var errorRec = new Error("IncorrectVatRegNo", "Please fill in a correct Vat Reg. No.", "VatRegistrationNo");
                errorArray.Add(errorRec);
                validFormat = false;
            }

            return validFormat;
        }




        /*private bool CustomerExists(int id)
        {
            return (_context.Customers?.Any(e => e.Id == id)).GetValueOrDefault();
        }*/


    }
}
 
