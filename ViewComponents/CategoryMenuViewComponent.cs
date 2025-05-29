using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Website_BanMayTinh.Repositories;

namespace Website_BanMayTinh.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryMenuViewComponent(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryRepository.GetCategoriesAsync();
            return View(categories);
        }
    }
}
