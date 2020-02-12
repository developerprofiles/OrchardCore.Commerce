using System.Collections.Generic;
using System.Threading.Tasks;
using Money;
using Money.Abstractions;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class PricePartDisplayDriver : ContentPartDisplayDriver<PricePart>
    {
        private readonly IMoneyService _moneyService;

        public PricePartDisplayDriver(IMoneyService moneyService)
        {
            _moneyService = moneyService;
        }

        public override IDisplayResult Display(PricePart pricePart)
        {
            return Combine(
                Initialize<PricePartViewModel>("PricePart", m => BuildViewModel(m, pricePart))
                    .Location("Detail", "Content:25"),
                Initialize<PricePartViewModel>("PricePart_Summary", m => BuildViewModel(m, pricePart))
                    .Location("Summary", "Meta:10")
            );
        }

        public override IDisplayResult Edit(PricePart pricePart)
        {
            return Initialize<PricePartViewModel>("PricePart_Edit", m => BuildViewModel(m, pricePart));
        }

        public override async Task<IDisplayResult> UpdateAsync(PricePart model, IUpdateModel updater)
        {
            var updateModel = new PricePartViewModel();
            await updater.TryUpdateModelAsync(updateModel, Prefix, t => t.PriceValue);
            await updater.TryUpdateModelAsync(updateModel, Prefix, t => t.PriceCurrency);
            model.Price = _moneyService.Create(updateModel.PriceValue, updateModel.PriceCurrency);

            return Edit(model);
        }

        private Task BuildViewModel(PricePartViewModel model, PricePart part)
        {
            model.ContentItem = part.ContentItem;

            model.Price = part.Price;
            model.PriceValue = part.Price.Value;
            model.PriceCurrency = part.Price.Currency == Currency.UnspecifiedCurrency ? _moneyService.DefaultCurrency.CurrencyIsoCode : part.Price.Currency.CurrencyIsoCode;
            model.PricePart = part;

            model.Currencies = GetCurrencySelectionList(part);

            return Task.CompletedTask;
        }

        private IEnumerable<ICurrency> GetCurrencySelectionList(PricePart part)
        {
            IEnumerable<ICurrency> currencySelectionList;

            switch (part.CurrencySelectionMode)
            {
                case CurrencySelectionModeEnum.DefaultCurrency:
                    currencySelectionList = new List<ICurrency>() { _moneyService.DefaultCurrency };
                    break;

                case CurrencySelectionModeEnum.SpecificCurrency:
                    currencySelectionList = new List<ICurrency>() { _moneyService.GetCurrency(part.CurrencyIsoCode) };
                    break;

                default:
                    // As a fallback show all currencies.
                    currencySelectionList = _moneyService.Currencies;
                    break;
            }

            return currencySelectionList;
        }
    }
}
