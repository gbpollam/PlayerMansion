using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Helpers;
using SandBox;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Overlay;
using PlayerMansion.Behaviors.NewSettlementAreas;

namespace PlayerMansion.Behaviors
{
    public class PlayerMansionCampaignBehavior : CampaignBehaviorBase
	{
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnAfterGameLoadedEvent));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(this.DailyTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<Dictionary<Settlement, Mansion>>("Mansions", ref Mansions);
        }

        public void OnAfterNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            this.NewAddGameMenus(campaignGameStarter);
            this.InitializeMansions(campaignGameStarter);
        }

        public void OnAfterGameLoadedEvent(CampaignGameStarter campaignGameStarter)
        {
            this.NewAddGameMenus(campaignGameStarter);
        }

        protected void NewAddGameMenus(CampaignGameStarter campaignGameSystemStarter)
        {
            campaignGameSystemStarter.AddGameMenuOption("town", "town_mansion", "{=!}{GO_TO_MANSION_TEXT}", new GameMenuOption.OnConditionDelegate(PlayerMansionCampaignBehavior.game_menu_town_mansion_on_condition), new GameMenuOption.OnConsequenceDelegate(PlayerMansionCampaignBehavior.game_menu_town_mansion_on_consequence), false, 4, false);
            campaignGameSystemStarter.AddGameMenu("town_mansion", "Town Mansion", (MenuCallbackArgs args) => { },
                GameOverlays.MenuOverlayType.SettlementWithBoth);
            campaignGameSystemStarter.AddGameMenuOption("town_mansion", "town_mansion_buy", "{=!}{BUY_MANSION_TEXT}", new GameMenuOption.OnConditionDelegate(PlayerMansionCampaignBehavior.game_menu_buy_mansion_on_condition), new GameMenuOption.OnConsequenceDelegate(PlayerMansionCampaignBehavior.game_menu_buy_mansion_on_consequence), false, -1, false);
            campaignGameSystemStarter.AddGameMenuOption("town_mansion", "town_mansion_leave", "{=3jiasdy}Leave", (MenuCallbackArgs args) => 
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, (MenuCallbackArgs args) => GameMenu.SwitchToMenu("town"), true, -1, false);
            campaignGameSystemStarter.AddGameMenu("town_mansion_owned", "Town Mansion", (MenuCallbackArgs args) => { },
                GameOverlays.MenuOverlayType.SettlementWithBoth);
            campaignGameSystemStarter.AddGameMenuOption("town_mansion_owned", "town_mansion_owned_party", "{=3bjbhgv}Manage the Household Guards", new GameMenuOption.OnConditionDelegate(PlayerMansionCampaignBehavior.game_menu_access_party_on_condition), new GameMenuOption.OnConsequenceDelegate(PlayerMansionCampaignBehavior.game_menu_access_party_on_consequence), false, -1, false);
            campaignGameSystemStarter.AddGameMenuOption("town_mansion_owned", "town_mansion_inventory", "{=3bjbhgv}Manage the Household Inventory", 
                new GameMenuOption.OnConditionDelegate(PlayerMansionCampaignBehavior.game_menu_access_inventory_on_condition), 
                new GameMenuOption.OnConsequenceDelegate(PlayerMansionCampaignBehavior.game_menu_access_inventory_on_consequence), false, -1, false);
            campaignGameSystemStarter.AddGameMenu("town_mansion_selling_screen", "Sell your mansion", (MenuCallbackArgs args) => { },
                GameOverlays.MenuOverlayType.SettlementWithBoth);
            campaignGameSystemStarter.AddGameMenuOption("town_mansion_owned", "town_mansion_presell", "{=3kjbfisb}Sell your mansion",
                new GameMenuOption.OnConditionDelegate(PlayerMansionCampaignBehavior.game_menu_presell_on_condition),
                new GameMenuOption.OnConsequenceDelegate(PlayerMansionCampaignBehavior.game_menu_presell_on_consequence), false, -1, false);
            campaignGameSystemStarter.AddGameMenuOption("town_mansion_selling_screen", "town_mansion_sell", "{=!}{SELL_MANSION_TEXT}",
                new GameMenuOption.OnConditionDelegate(PlayerMansionCampaignBehavior.game_menu_sell_on_condition),
                new GameMenuOption.OnConsequenceDelegate(PlayerMansionCampaignBehavior.game_menu_sell_on_consequence), false, -1, false);
            campaignGameSystemStarter.AddGameMenuOption("town_mansion_selling_screen", "town_mansion_selling_screen_leave", "{=3jiasdy}Leave", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, (MenuCallbackArgs args) => GameMenu.SwitchToMenu("town_mansion_owned"), true, -1, false);
            campaignGameSystemStarter.AddGameMenuOption("town_mansion_owned", "town_mansion_owned_leave", "{=3jiasdy}Leave", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, (MenuCallbackArgs args) => GameMenu.SwitchToMenu("town"), true, -1, false);
        }

		private static bool game_menu_town_mansion_on_condition(MenuCallbackArgs args)
		{
            TextObject text;
            if(Mansions[Settlement.CurrentSettlement].Owner != Hero.MainHero)
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                text = new TextObject("{=XZFQ1Jg8}Buy Mansion.", null);
            }
            else
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                text = new TextObject("{=XZFQ1Jh9}Visit your Mansion.", null);

            }
            MBTextManager.SetTextVariable("GO_TO_MANSION_TEXT", text, false);
            return true;
		}

        private static void game_menu_town_mansion_on_consequence(MenuCallbackArgs args)
		{
            if (Mansions[Settlement.CurrentSettlement].Owner != Hero.MainHero)
            {
                GameMenu.SwitchToMenu("town_mansion");
            }
            else
            {
                GameMenu.SwitchToMenu("town_mansion_owned");
            }
		}

        private static bool game_menu_buy_mansion_on_condition(MenuCallbackArgs args)
        {
            int price = Mansions[Settlement.CurrentSettlement].Price();
            TextObject text = new TextObject("{=XZFQ1Jg8}Buy the Mansion, which costs {PRICE}{GOLD_ICON}.", null);
            text.SetTextVariable("PRICE", price);
            text.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            MBTextManager.SetTextVariable("BUY_MANSION_TEXT", text, false);
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            return true;
        }

        private static void game_menu_buy_mansion_on_consequence(MenuCallbackArgs args)
        {
            int price = Mansions[Settlement.CurrentSettlement].Price();
            int purse = Hero.MainHero.Gold;
            if (purse >= price && Mansions[Settlement.CurrentSettlement].Owner != Hero.MainHero)
            {
                InformationManager.DisplayMessage(new InformationMessage("Congratulations, you bought the mansion!"));
                Hero.MainHero.ChangeHeroGold(-price);
                Mansions[Settlement.CurrentSettlement].SetMansion(Hero.MainHero);
                GameMenu.SwitchToMenu("town");
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("You don't have enough gold!"));
            }
        }

        private static bool game_menu_access_party_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.TroopSelection;
            if (!Mansions[Settlement.CurrentSettlement].IsRented)
            {
                return true;
            }
            return false;
        }

        private static void game_menu_access_party_on_consequence(MenuCallbackArgs args)
        {
            Mansions[Settlement.CurrentSettlement].OpenPartyScreenAsManageParty();
        }

        private static bool game_menu_access_inventory_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            if (!Mansions[Settlement.CurrentSettlement].IsRented)
            {
                return true;
            }
            return false;
        }

        private static void game_menu_access_inventory_on_consequence(MenuCallbackArgs args)
        {
            InventoryManager.OpenScreenAsStash(Mansions[Settlement.CurrentSettlement].ReturnHouseholdItemRoster);
        }

        private static bool game_menu_presell_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            if (!Mansions[Settlement.CurrentSettlement].IsRented)
            {
                return true;
            }
            return false;
        }

        private static void game_menu_presell_on_consequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu("town_mansion_selling_screen");
        }

        private static bool game_menu_sell_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            int price = Mansions[Settlement.CurrentSettlement].SellPrice();
            TextObject text = new TextObject("{=XZFQasdg8}Sell the mansion for {PRICE}{GOLD_ICON} (CAUTION: the remaining household guards and the inventory will be sold alongside it!)", null);
            text.SetTextVariable("PRICE", price);
            text.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            MBTextManager.SetTextVariable("SELL_MANSION_TEXT", text, false);
            if (Mansions[Settlement.CurrentSettlement].Owner == Hero.MainHero)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void game_menu_sell_on_consequence(MenuCallbackArgs args)
        {
            int price = Mansions[Settlement.CurrentSettlement].SellPrice();
            Hero.MainHero.ChangeHeroGold(price);
            Mansions[Settlement.CurrentSettlement].SellMansion();
            GameMenu.SwitchToMenu("town");
        }

        // Initializes one mansion in each town 
        private void InitializeMansions(CampaignGameStarter campaignGameStarter)
        {
            int count = 0;
            Mansions = new Dictionary<Settlement, Mansion>();
            foreach (Town town in Town.AllTowns)
            {
                Mansion mansion = new Mansion(town.Settlement, "mansion_" + count);
                Mansions.Add(town.Settlement, mansion);
            }
        }

        public void DailyTick()
        {
            int totWages = 0;
            int totExpenses = 0;
            Mansion currMansion = null;
            foreach (Town town in Town.AllTowns)
            {
                currMansion = Mansions[town.Settlement];
                if (currMansion.Owner == Hero.MainHero)
                {
                    totExpenses += currMansion.GetDailyRunningCosts();
                    totWages += currMansion.GetTroopsWages();
                }
            }
            totWages = Convert.ToInt32(totWages / 2);
            Hero.MainHero.ChangeHeroGold(-totExpenses);
            Hero.MainHero.ChangeHeroGold(-totWages);

            TextObject textObject = new TextObject("{=dPD5zood}Daily mansions running expenses (bis): {CHANGE}{GOLD_ICON}", null);
            textObject.SetTextVariable("CHANGE", totExpenses);
            textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            string soundEventPath = (totExpenses > 0) ? "event:/ui/notification/coins_positive" : ((totExpenses
                == 0) ? string.Empty : "event:/ui/notification/coins_negative");
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), soundEventPath));

            TextObject textObject1 = new TextObject("{=dPD5zood}Household guards wages (bis): {CHANGE}{GOLD_ICON}", null);
            textObject1.SetTextVariable("CHANGE", totWages);
            textObject1.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            string soundEventPath1 = (totWages > 0) ? "event:/ui/notification/coins_positive" : ((totWages
                == 0) ? string.Empty : "event:/ui/notification/coins_negative");
            InformationManager.DisplayMessage(new InformationMessage(textObject1.ToString(), soundEventPath1));

        }

        // Stores all the Mansions
        //old: public Mansion[] Mansions { get; protected set; }
        // [SaveableProperty(5)]
        // No need of SaveableField, used SyncData
        protected static Dictionary<Settlement, Mansion> Mansions;
    }
}
