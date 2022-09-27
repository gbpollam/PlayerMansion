using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace PlayerMansion.Behaviors.NewSettlementAreas
{
    public class Mansion : SettlementArea
    {
		public Mansion(Settlement settlement, string tag)
		{
			this._customName = null;
			this._settlement = settlement;
			this._tag = tag;
			this._dailyRunningCosts = 100;
			this._price = 15;
		}

		public void SetMansion(Hero newOwner, TextObject customName = null)
		{
			this._owner = newOwner;
			this._customName = customName;
			this._isRented = false;
		}

		public override Settlement Settlement
		{
			get
			{
				return this._settlement;
			}
		}

		public override string Tag
		{
			get
			{
				return this._tag;
			}
		}

		public override Hero Owner
		{
			get
			{
				return this._owner;
			}
		}

		public override TextObject Name
		{
			get
			{
				return this._customName;
			}
		}

		public int Price()
		{
				return this._price;
		}

		public int SellPrice()
		{
				return Convert.ToInt32(this._price*(2/3));
		}

		public void OpenPartyScreenAsManageParty()
		{
			PartyScreenManager.OpenScreenAsManageTroops(this.HouseholdParty());
		}

		private MobileParty HouseholdParty()
        {
			if (this._householdParty == null)
            {
				this._householdParty = new MobileParty();
				this._householdParty.IsActive = false;
				TextObject text = new TextObject("{=XZFQj4Jg8}Household Party.", null);
				this._householdParty.SetCustomName(text);
			}
			return this._householdParty;
        }

		public ItemRoster ReturnHouseholdItemRoster
		{
			get
			{
				if (this.HouseholdItemRoster == null)
                {
					this.HouseholdItemRoster = new ItemRoster();
                }
				return this.HouseholdItemRoster;
			}
		}

		public int GetDailyRunningCosts()
        {
			return this._dailyRunningCosts;
        }

		public int GetTroopsWages()
        {
			if (this._householdParty != null)
				return this._householdParty.TotalWage;
			else
				return 0;
        }

		public void SellMansion()
        {
			this._owner = null;
			this._customName = null;
			this._householdParty = null;
			this.HouseholdItemRoster = null;
        }

		public bool IsRented
        {
			get
            {
				return this._isRented;
            }
			set
            {
				this._isRented = value;
            }
        }

		[SaveableField(1)]
		private Settlement _settlement;

		[SaveableField(2)]
		private readonly string _tag;

		[SaveableField(3)]
		private Hero _owner;

		[SaveableField(4)]
		private TextObject _customName;

		[SaveableField(6)]
		private int _price;

		[SaveableField(7)]
		private MobileParty _householdParty;

		[SaveableProperty(5)]
		public ItemRoster HouseholdItemRoster { get; private set; }

		[SaveableField(8)]
		private int _dailyRunningCosts;

		[SaveableField(9)]
		private bool _isRented;
	}
}
