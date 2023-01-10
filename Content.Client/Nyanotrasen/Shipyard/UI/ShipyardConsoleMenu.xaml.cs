using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Client.Shipyard.BUI;
using Content.Shared.Shipyard.BUI;
using Content.Shared.Shipyard.Prototypes;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.Shipyard.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class ShipyardConsoleMenu : FancyWindow
    {
        private IPrototypeManager _protoManager;
        private SpriteSystem _spriteSystem;
        public event Action<ButtonEventArgs>? OnOrderApproved;
        private readonly ShipyardConsoleBoundUserInterface _menu;
        private readonly List<string> _categoryStrings = new();
        private string? _category;
        private readonly List<string> _accessList = new();

        public ShipyardConsoleMenu(ShipyardConsoleBoundUserInterface owner, IPrototypeManager protoManager, SpriteSystem spriteSystem, List<string> accessList)
        {
            RobustXamlLoader.Load(this);
            _protoManager = protoManager;
            _spriteSystem = spriteSystem;
            _accessList = accessList;
            _menu = owner;
            Title = Loc.GetString("shipyard-console-menu-title");
            SearchBar.OnTextChanged += OnSearchBarTextChanged;
            Categories.OnItemSelected += OnCategoryItemSelected;
//            SellShipButton.OnPressed += (args) => { OnSellShip?.Invoke(args); };
        }

        private void OnCategoryItemSelected(OptionButton.ItemSelectedEventArgs args)
        {
            SetCategoryText(args.Id);
            PopulateProducts();
        }

        private void OnSearchBarTextChanged(LineEdit.LineEditEventArgs args)
        {
            PopulateProducts();
        }

        private void SetCategoryText(int id)
        {
            _category = id == 0 ? null : _categoryStrings[id];
            Categories.SelectId(id);
        }

        public IEnumerable<VesselPrototype> VesselPrototypes => _protoManager.EnumeratePrototypes<VesselPrototype>();

        /// <summary>
        ///     Populates the list of products that will actually be shown, using the current filters.
        /// </summary>
        public void PopulateProducts()
        {
            Vessels.RemoveAllChildren();
            var vessels = VesselPrototypes.ToList();
            vessels.Sort((x, y) =>
                string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase));

            var search = SearchBar.Text.Trim().ToLowerInvariant();
            foreach (var prototype in vessels)
            {
                // if no search or category
                // else if search
                // else if category and not search
                if (search.Length == 0 && _category == null ||
                    search.Length != 0 && prototype.Name.ToLowerInvariant().Contains(search) ||
                    search.Length == 0 && _category != null && prototype.Category.Equals(_category))
                {
                    var vesselEntry = new VesselRow
                    {
                        Vessel = prototype,
                        VesselName = { Text = prototype.Name },
                        Purchase = { ToolTip = prototype.Description },
                        Price = { Text = Loc.GetString("cargo-console-menu-points-amount", ("amount", prototype.Price.ToString())) },
                    };
                    vesselEntry.Purchase.OnPressed += (args) => { OnOrderApproved?.Invoke(args); };
                    Vessels.AddChild(vesselEntry);
                }
            }
        }

        /// <summary>
        ///     Populates the list categories that will actually be shown, using the current filters.
        /// </summary>
        public void PopulateCategories()
        {
            _categoryStrings.Clear();
            Categories.Clear();

            foreach (var prototype in VesselPrototypes)
            {
                if (!_categoryStrings.Contains(prototype.Category))
                {
                    _categoryStrings.Add(Loc.GetString(prototype.Category));
                }
            }

            _categoryStrings.Sort();

            // Add "All" category at the top of the list
            _categoryStrings.Insert(0, Loc.GetString("cargo-console-menu-populate-categories-all-text"));

            foreach (var str in _categoryStrings)
            {
                Categories.AddItem(str);
            }
        }

        public void UpdateState(ShipyardConsoleInterfaceState state)
        {
            BankAccountLabel.Text = Loc.GetString("cargo-console-menu-points-amount", ("amount", state.Balance.ToString()));

//            SellShipButton.Disabled = state.ShipDeedTitle == null;
//            if (state.ShipDeedTitle != null)
//            {
//                DeedTitle.Text = state.ShipDeedTitle;
 //           }

        }
    }
}
