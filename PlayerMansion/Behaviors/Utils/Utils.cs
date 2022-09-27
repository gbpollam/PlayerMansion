using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace PlayerMansion.Behaviors.Utils
{
    class InventoryManagerUtils : InventoryManager
    {
		public static void OpenScreenAsStash(ItemRoster stash)
		{
			//InventoryManager.Instance._currentMode = InventoryMode.Stash;
			//InventoryManager.Instance._inventoryLogic = new InventoryLogic(null);
			//InventoryManager.Instance._inventoryLogic.Initialize(stash, MobileParty.MainParty, false, false, CharacterObject.PlayerCharacter, InventoryManager.InventoryCategoryType.None, InventoryManager.GetCurrentMarketData(), false, new TextObject("{=nZbaYvVx}Stash", null));
			InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
			//inventoryState.InitializeLogic(InventoryManager.Instance._inventoryLogic);
			Game.Current.GameStateManager.PushState(inventoryState, 0);
		}
	}
}
