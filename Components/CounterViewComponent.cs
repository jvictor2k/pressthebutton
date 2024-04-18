using Microsoft.AspNetCore.Mvc;

namespace PressTheButton.Components
{
    public class CounterViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int infoNum)
        {
            return View(infoNum);
        }
    }
}
