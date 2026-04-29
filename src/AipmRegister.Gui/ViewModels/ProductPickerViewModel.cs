using System.Collections.ObjectModel;
using AipmRegister.Core.Models;
using AipmRegister.Gui.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class ProductPickerViewModel : ObservableObject
{
    private readonly IWizardNavigator _nav;
    private readonly WizardState _state;

    public ProductPickerViewModel(IWizardNavigator nav, WizardState state)
    {
        _nav = nav;
        _state = state;
        Products = new ObservableCollection<ProductDefinition>(ProductCatalog.All);
    }

    public ObservableCollection<ProductDefinition> Products { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private ProductDefinition? selected;

    partial void OnSelectedChanged(ProductDefinition? value)
    {
        if (value is not null) _state.Product = value;
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    private void Next() => _nav.Go(WizardStep.DevicePicker);

    private bool CanNext() => Selected is not null;
}
