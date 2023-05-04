using Microsoft.AspNetCore.Mvc.Rendering;
using PE.BL.Services;

namespace SW.InternalWeb.Services;

public class EmailTypeSelectItemsService
{
    private readonly ICodeService codeService;

    public EmailTypeSelectItemsService(ICodeService codeService)
    {
        this.codeService = codeService;
    }

    public async Task<IEnumerable<SelectListItem>> Get()
    {
        return (await codeService.GetByType("Email", false))
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code1} - {c.Description}"
            })
            .ToList();
    }
}
