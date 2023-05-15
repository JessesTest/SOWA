using Microsoft.AspNetCore.Mvc.Rendering;
using PE.BL.Services;

namespace SW.InternalWeb.Services;

public class PhoneTypeSelectItemsService
{
    private readonly ICodeService codeService;

    public PhoneTypeSelectItemsService(ICodeService codeService)
    {
        this.codeService = codeService;
    }

    public async Task<IEnumerable<SelectListItem>> Get()
    {
        var codes = await codeService.GetByType("Phone", false);
        return codes
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code1} - {c.Description}"
            })
            .ToList();
    }
}
