using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PressTheButton.Context;
using PressTheButton.Models;
using PressTheButton.ViewModels;

namespace PressTheButton.Controllers
{
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuestionController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var questions = await _context.Questions.Where(q => q.CreatedBy == userId).ToListAsync();
            return View(questions);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == id);

                if(question != null)
                {
                    var userId = _userManager.GetUserId(User);
                    if(question.CreatedBy == userId)
                    {
                        return View(question);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        //GET method Create
        public IActionResult Create()
        {
            return View();
        }

        //POST method Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    question.CreatedBy = _userManager.GetUserId(User);
                    question.Date = DateTime.Now;
                    question.Ativo = true;
                    _context.Add(question);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Lidar com exceções, se necessário
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar o item.");
            }

            return View(question);
        }

        //GET method Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == id);

                if(question != null)
                {
                    var userId = _userManager.GetUserId(User);
                    if(question.CreatedBy == userId)
                    {
                        return View(question);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        //POST method Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Question question)
        {
            if(id != question.QuestionId)
            {
                return NotFound();
            }
            else
            {
                if(ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(question);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!QuestionExists(question.QuestionId))
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
                return View(question);
            }
        }

        //GET method Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == id);

                if(question != null)
                {
                    return View(question);
                }
                else 
                { 
                    return NotFound(); 
                }
            }
            else
            {
                return NotFound();
            }
        }

        //POST method Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.Questions.FindAsync(id);

            if(question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AllResponses()
        {
            var userId = _userManager.GetUserId(User);

            if(userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var questionsWithResponses = _context.Questions
            .Where(q => q.UserResponses.Any(ur => ur.UserId == userId))
            .Select(q => new QuestionUserResponseViewModel
            {
                QuestionId = q.QuestionId,
                Text = q.Text,
                Negative = q.Negative,
                YesOrNo = q.UserResponses.FirstOrDefault(ur => ur.UserId == userId).YesOrNo
            })
            .ToList();

            return View(questionsWithResponses);
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(q => q.QuestionId == id);
        }
    }
}
