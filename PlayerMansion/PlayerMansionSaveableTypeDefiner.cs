using PlayerMansion.Behaviors;
using System.Collections.Generic;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem;
using PlayerMansion.Behaviors.NewSettlementAreas;
using TaleWorlds.CampaignSystem.Settlements;

namespace PlayerMansion
{
    public class PlayerMansionSaveableTypeDefiner : SaveableTypeDefiner
    {
        public PlayerMansionSaveableTypeDefiner() : base(563413242)
        {
        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(Mansion), 1);
            AddClassDefinition(typeof(PlayerMansionCampaignBehavior), 2);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<Settlement, Mansion>));
        }
    }
}
