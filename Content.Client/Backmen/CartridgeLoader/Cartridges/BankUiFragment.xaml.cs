﻿using System.Linq;
using Content.Client.Backmen.Economy;
using Content.Client.Message;
using Content.Shared.Backmen.CartridgeLoader.Cartridges;
using Content.Shared.Backmen.Economy;
using Content.Shared.Store;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.Backmen.CartridgeLoader.Cartridges;

[GenerateTypedNameReferences]
public sealed partial class BankUiFragment : BoxContainer
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private readonly TransformSystem _transformSystem;
    private readonly ContainerSystem _containerSystem;

    public BankAccountComponent? Bank;
    private CurrencyPrototype? _currencyPrototype;

    public BankUiFragment()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        _transformSystem = _entityManager.System<TransformSystem>();
        _containerSystem = _entityManager.System<ContainerSystem>();
    }
    public void UpdateState(BankUiState state)
    {
        FillFields();
        /*
        var noAccount = state.LinkedAccountNumber == null;

        NoLinkedAccount.Visible = noAccount;
        LinkedAccount.Visible = !noAccount;
        if (noAccount)
        {
            NoLinkedAccount.SetMarkup(Loc.GetString("bank-program-no-account"));
        }
        else
        {
            LinkedAccountNumberLabel.SetMarkup(Loc.GetString("bank-program-account-number", ("number", state.LinkedAccountNumber ?? "???")));
            LinkedAccountNameLabel.SetMarkup(Loc.GetString("bank-program-account-name", ("name", state.LinkedAccountName ?? "???")));
            LinkedAccountBalanceLabel.SetMarkup(Loc.GetString("bank-program-account-balance", ("balance", state.LinkedAccountBalance ?? 0), ("currencySymbol", state.CurrencySymbol ?? "")));
        }
*/

        LinkedAccountBalanceLabel.SetMarkup(Loc.GetString("bank-program-account-balance", ("balance", state.LinkedAccountBalance ?? 0), ("currencySymbol", Loc.GetString(_currencyPrototype?.CurrencySymbol ?? ""))));
    }

    private void NoAccount()
    {
        NoLinkedAccount.Visible = true;
        LinkedAccount.Visible = false;
    }

    public void UpdateBankInfo()
    {
        LinkedAccountNumberLabel.SetMarkup(Loc.GetString("bank-program-account-number", ("number", Bank?.AccountNumber ?? "???")));
        LinkedAccountNameLabel.SetMarkup(Loc.GetString("bank-program-account-name", ("name", Bank?.AccountName ?? "???")));
        if (Bank != null && _prototypeManager.TryIndex(Bank.CurrencyType, out _currencyPrototype))
        {

        }
        LinkedAccountBalanceLabel.SetMarkup(Loc.GetString("bank-program-account-balance", ("balance", Bank?.Balance ?? 0), ("currencySymbol", Loc.GetString(_currencyPrototype?.CurrencySymbol ?? ""))));
    }

    public void FillFields()
    {
        NoLinkedAccount.SetMarkup(Loc.GetString("bank-program-no-account"));
        if (_fragmentOwner == null)
        {
            NoAccount();
            return;
        }
        var pda = _transformSystem.GetParentUid(_fragmentOwner.Value);
        if (!pda.IsValid())
        {
            NoAccount();
            return;
        }

        if (!_containerSystem.TryGetContainer(pda, "PDA-id", out var idCardContainer) || idCardContainer.Count == 0)
        {
            NoAccount();
            return;
        }

        var idCard = idCardContainer.ContainedEntities.First();

        if (!_entityManager.TryGetComponent(idCard, out Bank))
        {
            NoAccount();
            return;
        }

        if (Bank == null)
        {
            NoAccount();
            return;
        }

        NoLinkedAccount.Visible = false;
        LinkedAccount.Visible = true;
        UpdateBankInfo();
    }

    private EntityUid? _fragmentOwner;
    public void UpdateEntity(EntityUid? fragmentOwner)
    {
        _fragmentOwner = fragmentOwner;
        FillFields();
    }
}