using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManager.Areas.Identity.Data;
using UserManager.Data;

namespace UserManager.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManagerContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserController(UserManagerContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: UserController
        public async Task<ActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }


        private List<string> list = new List<string>();
        [HttpPost]
        public async Task<IActionResult> SelectItem(string id)
        {
            list.Add(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: UserController/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (user.LockoutEnd > DateTime.Today)
            {
                user.LockoutEnd = null;//DateTime.Today;
                user.LockoutEnabled = false;

            }
            else
            {
                user.LockoutEnd = DateTime.Today.AddYears(1);
                user.LockoutEnabled = true;
                await _userManager.UpdateSecurityStampAsync(user);
            }

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExist(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (user.Email == User.Identity.Name && user.LockoutEnabled == true)
            {
                await Logout();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private bool UserExist(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ID,Name,LastName,Email,RegDate,LastLogDate,Lockedout")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExist(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: UserController/Delete/5
        public async Task<IActionResult> Delete(string id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            await Logout();

            return RedirectToAction(nameof(Index));

        }

        // POST: UserController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var webApplication15User1 = await _context.Users.FindAsync(id);
            _context.Users.Remove(webApplication15User1);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
