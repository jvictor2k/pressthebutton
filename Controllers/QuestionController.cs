using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PressTheButton.Context;
using PressTheButton.Models;

namespace PressTheButton.Controllers
{
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;

        public QuestionController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var questions = await _context.Questions.ToListAsync();
            return View(questions);
        }

        public async Task<IActionResult> Details(int? id)
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
            if(!ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(q => q.QuestionId == id);
        }
    }
}
