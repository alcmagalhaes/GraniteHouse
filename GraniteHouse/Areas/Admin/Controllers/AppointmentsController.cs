using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models;
using GraniteHouse.Models.ViewModels;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.AdminEndUser + "," + SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        //GET : 
        public async Task<IActionResult> Index(string searchName = null, string searchEmail = null, string searchPhone = null, string searchDate = null)
        {
            ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentsViewModel appointmentVM = new AppointmentsViewModel()
            {
                Appointments = new List<Models.Appointments>()
            };

            appointmentVM.Appointments = await _db.Appointments.Include(a => a.SalesPerson).ToListAsync();
            if (User.IsInRole(SD.AdminEndUser))
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.SalesPersonId == claim.Value).ToList();
            }

            if (searchName != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerName.ToLower().Contains(searchName.ToLower())).ToList();
            }
            if (searchEmail != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerEmail.ToLower().Contains(searchEmail.ToLower())).ToList();
            }
            if (searchPhone != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerPhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToList();
            }
            if (searchDate != null)
            {
                try
                {
                    DateTime appDate = Convert.ToDateTime(searchDate);
                    appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.AppointmentDate.ToShortDateString().Equals(appDate.ToShortDateString())).ToList();
                }
                catch(Exception ex)
                {

                }
            }

            return View(appointmentVM);
        }

        //GET : Edit 
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var products = (IEnumerable<Products>)(from p in _db.Products
                                                   join a in _db.ProductsSelectedForAppointments
                                                   on p.Id equals a.ProductId
                                                   where a.AppointmentId == id
                                                   select p).Include("ProductTypes");

            AppointmentsDetailsViewModel appointmentsDetailsVM = new AppointmentsDetailsViewModel()
            {
                Appointments = _db.Appointments.Include(a => a.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPersons = _db.ApplicationUser.ToList(),
                Products = products.ToList()
            };

            return View(appointmentsDetailsVM);
        }

        //POST : Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentsDetailsViewModel objAppointmentVM)
        {
            if (ModelState.IsValid)
            {
                objAppointmentVM.Appointments.AppointmentDate = objAppointmentVM.Appointments.AppointmentDate
                                        .AddHours(objAppointmentVM.Appointments.AppointmentTime.Hour)
                                        .AddMinutes(objAppointmentVM.Appointments.AppointmentTime.Minute);

                var appointmentFromDb = _db.Appointments.Where(a => a.Id == objAppointmentVM.Appointments.Id).FirstOrDefault();

                appointmentFromDb.CustomerName = objAppointmentVM.Appointments.CustomerName;
                appointmentFromDb.CustomerEmail = objAppointmentVM.Appointments.CustomerEmail;
                appointmentFromDb.CustomerPhoneNumber = objAppointmentVM.Appointments.CustomerPhoneNumber;
                appointmentFromDb.AppointmentDate = objAppointmentVM.Appointments.AppointmentDate;
                appointmentFromDb.isConfirmed = objAppointmentVM.Appointments.isConfirmed;
                if (User.IsInRole(SD.SuperAdminEndUser))
                {
                    appointmentFromDb.SalesPersonId = objAppointmentVM.Appointments.SalesPersonId;
                }

                _db.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(objAppointmentVM);
        }

        //GET : Details 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var products = (IEnumerable<Products>)(from p in _db.Products
                                                   join a in _db.ProductsSelectedForAppointments
                                                   on p.Id equals a.ProductId
                                                   where a.AppointmentId == id
                                                   select p).Include("ProductTypes");

            AppointmentsDetailsViewModel appointmentsDetailsVM = new AppointmentsDetailsViewModel()
            {
                Appointments = _db.Appointments.Include(a => a.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPersons = _db.ApplicationUser.ToList(),
                Products = products.ToList()
            };

            return View(appointmentsDetailsVM);
        }
    }
}