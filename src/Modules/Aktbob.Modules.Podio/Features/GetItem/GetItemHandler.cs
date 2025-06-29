using AAK.Podio;
using AAK.Podio.Models;
using AktBob.Shared.Contracts.Modules.Podio;
using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.GetItem;
internal class GetItemHandler(IPodioFactory podioFactory, IConfigurationHelper configurationHelper) : IGetItemHandler
{
    public async Task<ErrorOr<ItemDto>> Handle(ItemId itemId, CancellationToken cancellationToken)
    {
        var podio = podioFactory.Create(
            appId: itemId.AppId, 
            appToken: configurationHelper.GetAppToken(itemId.AppId), 
            clientId: configurationHelper.ClientId,
            clientSecret: configurationHelper.GetClientSecret);

        var item = await podio.GetItem(itemId.AppId, itemId.Id, cancellationToken);
        if (item == null)
        {
            return Error.NotFound("Podio.ItemNotFound", $"Item {itemId} not found.");
        }

        var dto = new ItemDto(
            Id: item.ItemId,
            AppId: item.AppId,
            Fields: item.Fields.Select(ToFieldDto).ToArray());
        
        return dto;
    }

    private static FieldDto ToFieldDto(Field field)
    {
        object? value = null;
        
        switch (field.Type)
        {
            case FieldType.Category:
                value = field.GetValues<FieldValueCategory>()?.Categories.Select(x => new CategoryValueDto(x.OptionId, x.Value)).ToArray<object>();
                break;
            
            case FieldType.Text:
                value = field.GetValues<FieldValueText>()?.Value;
                break;
            
            case FieldType.Contact:
                value = field.GetValues<FieldValueContact>()?.Contacts.Select(x => new ContactValueDto(x.Name, x.Email)).ToArray<object>();
                break;
            
            case FieldType.DateTime:
                var fieldValue = field.GetValues<FieldValueDateTime>();
                value = new DateTimeValueDto(fieldValue?.Start, fieldValue?.End);
                break;
            
            case FieldType.Number:
                var numberString = field.GetValues<FieldValueNumber>()?.Value;
                if (float.TryParse(numberString, out var number))
                {
                    value = number;
                }
                break;
            
            case FieldType.Calculation:
                value = field.GetValues<FieldValueCalculation>()?.Value;
                break;
            
            case FieldType.Embed:
                value = field.GetValues<FieldValueEmbed>()?.Value;
                break;
                
            // Not used in Aktbob at the moment
            case FieldType.Unknown:
            case FieldType.Location:
            case FieldType.App:
            case FieldType.Image:
            default:
                break;
        }
        
        var dto = new FieldDto(
            Id: field.Id,
            Label: field.Label,
            Value: value);
        
        return dto;
    }
}