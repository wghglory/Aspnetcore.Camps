# AutoMapping

Previously, we created a viewModel by entity, vice versa. In put request, we have viewModel from form, and we also have entity which comes from database and to be updated. So autoMapper here, is to map source(viewModel) to destination(entity):

```csharp
_mapper.Map(model, oldCamp);
```

Thus, entity is updated. Then we can save it to db.

```csharp
[HttpPut("{moniker}")]
public async Task<IActionResult> Put(string moniker, [FromBody] CampViewModel model)
{
    try
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var oldCamp = _repo.GetCampByMoniker(moniker);
        if (oldCamp == null) return NotFound($"Could not find a camp with an Moniker of {moniker}");

        // note: `property: null` from viewModel will make entity `property: null` even if entity previous property has a value
        _mapper.Map(model, oldCamp);

        if (await _repo.SaveAllAsync())
        {
            return Ok(_mapper.Map<CampViewModel>(oldCamp));
        }
    }
    catch (Exception)
    {
        // ignored
    }

    return BadRequest("Couldn't update Camp");
}

[HttpPatch("{moniker}")]
public async Task<IActionResult> Patch(string moniker, [FromBody] JsonPatchDocument<CampViewModel> patchDoc)
{
    try
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var oldCamp = _repo.GetCampByMoniker(moniker);
        if (oldCamp == null) return NotFound($"Could not find a camp with an Moniker of {moniker}");

        CampViewModel model = _mapper.Map<CampViewModel>(oldCamp);

        patchDoc.ApplyTo(model, ModelState);

        TryValidateModel(model);

        if (!ModelState.IsValid) return BadRequest(ModelState);

        _mapper.Map(model, oldCamp);

        if (await _repo.SaveAllAsync())
        {
            return Ok(_mapper.Map<CampViewModel>(oldCamp));
        }
    }
    catch (Exception)
    {
        // ignored
    }

    return BadRequest("Couldn't update Camp");
}

[HttpDelete("{moniker}")]
public async Task<IActionResult> Delete(string moniker)
{
    try
    {
        var oldCamp = _repo.GetCampByMoniker(moniker);
        if (oldCamp == null) return NotFound($"Could not find Camp with Moniker of {moniker}");

        _repo.Delete(oldCamp);
        if (await _repo.SaveAllAsync())
        {
            return Ok();
        }
    }
    catch (Exception)
    {
        // ignored
    }

    return BadRequest("Could not delete Camp");
}
```