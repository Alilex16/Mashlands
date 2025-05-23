using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace MashlandsNS
{
    public class Mashlands : Mod
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CardCapIncrease))]
        public static void WorldManager__Testing(WorldManager __instance, GameBoard board, ref int __result)
        {
            __result += __instance.GetCardCount("mashlands_structure_distribution_centre", board) * 50;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EndOfMonthCutscenes), nameof(EndOfMonthCutscenes.SpecialEvents))]
        public static void SpecialEvent__Testing()
        {
            bool spawnWanderingTrader = (UnityEngine.Random.value <= 0.1f &&  WorldManager.instance.CurrentMonth >= 8 &&  WorldManager.instance.CurrentMonth % 2 == 0) ||  WorldManager.instance.CurrentMonth == 24;

            if (spawnWanderingTrader)
            {
                Vector3 pos_random = WorldManager.instance.GetRandomSpawnPosition();
                CardData cardData0 = WorldManager.instance.CreateCard(pos_random, "mashlands_wandering_trader", faceUp: true, checkAddToStack: false);
                GameCamera.instance.TargetPositionOverride = cardData0.transform.position;
            }
        }

        public void Awake()
        {
            Harmony.PatchAll(typeof(TestingMod));

            SokLoc.instance.LoadTermsFromFile(System.IO.Path.Combine(this.Path, "localization.tsv"));
        }

        public override void Ready()
        {
            // Seeking Wisdom(1)
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.BasicIdea, "mashlands_blueprint_executing_villager", 1);

            // Island of Ideas(1), Island of Ideas(4)
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.Island_BasicIdea, "mashlands_blueprint_trident_storm", 1);

            // Seeking Wisdom(3), Reap&Sow(3), Logic and Reason(1)
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.BasicBuildingIdea, "mashlands_blueprint_library", 1);

            //Logic and Reason(4)
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "mashlands_blueprint_coin_vault", 1);
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "mashlands_blueprint_deconstruction_table", 1);
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "mashlands_blueprint_distribution_centre", 1);

            //The Armory(1)
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.EquipmentBlueprints, "mashlands_blueprint_leather_cap", 1);
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.EquipmentBlueprints, "mashlands_blueprint_spiked_shield", 1);
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.EquipmentBlueprints, "mashlands_blueprint_composite_bow", 1);
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.EquipmentBlueprints, "mashlands_blueprint_steel_chainplate", 1);


            Harvestable old_village = (Harvestable)WorldManager.instance.GetCardPrefab("old_village");
		    old_village.MyCardBag.Chances.Add(new CardChance("mashlands_resource_leather", 2));
		    old_village.MyCardBag.Chances.Add(new CardChance("mashlands_location_castle_ruins", 1));

            Harvestable plains = (Harvestable)WorldManager.instance.GetCardPrefab("plains");
		    plains.MyCardBag.Chances.Add(new CardChance("mashlands_location_castle_ruins", 1));

            Harvestable graveyard = (Harvestable)WorldManager.instance.GetCardPrefab("graveyard");
		    graveyard.MyCardBag.Chances.Add(new CardChance("mashlands_rumor_decompose_corpse", 2));

            Harvestable mountain = (Harvestable)WorldManager.instance.GetCardPrefab("mountain");
		    mountain.MyCardBag.Chances.Add(new CardChance("mashlands_animal_guinea_pig", 1));

            Harvestable forest = (Harvestable)WorldManager.instance.GetCardPrefab("forest");
		    forest.MyCardBag.Chances.Add(new CardChance("mashlands_animal_guinea_pig", 3));


            Harvestable berrybush = (Harvestable)WorldManager.instance.GetCardPrefab("berrybush");
		    berrybush.MyCardBag.Chances.Add(new CardChance("mashlands_animal_guinea_pig", 3));

            Harvestable grape_vine = (Harvestable)WorldManager.instance.GetCardPrefab("grape_vine");
		    grape_vine.MyCardBag.Chances.Add(new CardChance("mashlands_animal_guinea_pig", 3));

            Logger.Log("Mashlands Ready!");
        }
    }


    public class GuineaPig : Animal
    {
        [ExtraData("has_clicked")]
        public bool canClickPet = true;

        public override void Clicked()
        {
            if (!InAnimalPen)
            {
                if (!MyGameCard.Velocity.HasValue)
                {
                    MoveTimer = MoveTime;
                }

                if (!MyGameCard.TimerRunning && canClickPet)
                {
                    canClickPet = false;
                    
                    if (WorldManager.instance.CurrentBoard.Id == "island")
                    {
                        CardData cardData1 = WorldManager.instance.CreateCard(MyGameCard.transform.position, "grape", faceUp: true, checkAddToStack: true);
                        WorldManager.instance.CreateSmoke(cardData1.transform.position);
                    }
                    else
                    {
                        CardData cardData2 = WorldManager.instance.CreateCard(MyGameCard.transform.position, "berry", faceUp: true, checkAddToStack: true);
                        WorldManager.instance.CreateSmoke(cardData2.transform.position);
                    }
                    MyGameCard.SendIt();
                }
                base.Clicked();
            }
        }

        public override void UpdateCard()
        {
            base.UpdateCard();

            if (!MyGameCard.TimerRunning && !canClickPet)
            {
                MyGameCard.StartTimer(60f, PettingGuineaPig, SokLoc.Translate("action_petting_guinea_pig_status"), MyGameCard.CardData.GetActionId("PettingGuineaPig"));
            }
            if (canClickPet)
            {
                MyGameCard.CancelTimer(GetActionId("PettingGuineaPig"));
            }
        }

        [TimedAction("petting_guinea_pig")]
        public void PettingGuineaPig()
        {
            MyGameCard.SendIt();
            canClickPet = true;
        }
    }


    public class Crusher : CardData
    {
        public override bool DetermineCanHaveCardsWhenIsRoot => true;

        public override bool CanHaveCardsWhileHasStatus()
        {
            return true;
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            return otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Equipable;
        }
    }


    public class PressureChamber : CardData
    {
        public override bool DetermineCanHaveCardsWhenIsRoot => true;

        public override bool CanHaveCardsWhileHasStatus()
        {
            return true;
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            return otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Equipable;
        }
    }


    public class DeconstructionTable : CardData
    {
        public override bool DetermineCanHaveCardsWhenIsRoot => true;

        public override bool CanHaveCardsWhileHasStatus()
        {
            return true;
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            return otherCard.MyCardType == CardType.Equipable;
        }
    }

    
    public class DistributionCentre : CardData
    {
        protected override bool CanHaveCard(CardData otherCard)
        {
            return otherCard.Id == "shed" || otherCard.Id == "warehouse" || otherCard.Id == "lighthouse" || otherCard.Id == "mashlands_structure_distribution_centre";
        }
    }


    public class Library : CardData
    {
        [ExtraData("timer_temp")]
        public float timerTemp;

        public int ReadingCost = 5;
        public float ReadingTime = 150f;

        [Card]
        public List<string> BlueprintDrops = new List<string>();

        // public List<AudioClip> InventionSound;

        [ExtraData("coin_count")]
        [HideInInspector]
        public int CoinCount;

        private string HeldCardId = "gold";

        public void Awake()
        {
            BlueprintDrops = ["mashlands_blueprint_bone_dust", "mashlands_blueprint_flint", "mashlands_blueprint_steel_bar", "mashlands_blueprint_mithril_bar"];
            Extensions.Shuffle(BlueprintDrops);
        }

        
        protected override bool CanHaveCard(CardData otherCard)
        {
            if (!AllBlueprintsFound())
            {
                if (!(otherCard.Id == HeldCardId))
                {
                    if (otherCard is Chest chest)
                    {
                        return chest.HeldCardId == HeldCardId;
                    }
                    return otherCard.MyCardType == CardType.Humans; // 
                    // return false;
                }
                return true;
            }
            return false;
        }

        public override void UpdateCardText()
        {
            if (AllBlueprintsFound())
            {
                descriptionOverride = SokLoc.Translate("mashlands_structure_library_description_completed");
            }
            else if (CoinCount > 0)
            {
                descriptionOverride = SokLoc.Translate("mashlands_structure_library_description_long", LocParam.Create("count", CoinCount.ToString()), LocParam.Create("max_count", ReadingCost.ToString()));
            }
            else
            {
                descriptionOverride = SokLoc.Translate("mashlands_structure_library_description", LocParam.Create("max_count", ReadingCost.ToString()));
            }
        }

        public override void UpdateCard()
        {
            if (!MyGameCard.HasParent || MyGameCard.Parent.CardData is HeavyFoundation)
            {
                foreach (GameCard childCard in MyGameCard.GetChildCards())
                {
                    if (childCard.CardData is Chest chest)
                    {
                        if (chest.CoinCount < ReadingCost - CoinCount)
                        {
                            CoinCount += chest.CoinCount;
                            chest.CoinCount = 0;
                            WorldManager.instance.CreateSmoke(MyGameCard.transform.position);
                            chest.MyGameCard.RemoveFromStack();
                            chest.MyGameCard.SendIt();
                        }
                        else if (chest.CoinCount >= ReadingCost - CoinCount)
                        {
                            chest.CoinCount -= ReadingCost - CoinCount;
                            CoinCount = ReadingCost;
                            WorldManager.instance.CreateSmoke(MyGameCard.transform.position);
                            chest.MyGameCard.RemoveFromStack();
                            chest.MyGameCard.SendIt();
                        }
                    }
                    if (!(childCard.CardData.Id != HeldCardId))
                    {
                        if (CoinCount >= ReadingCost)
                        {
                            childCard.RemoveFromParent();
                            break;
                        }
                        childCard.DestroyCard(spawnSmoke: true);
                        CoinCount++;
                    }
                }
                
                if (CoinCount == ReadingCost)
                {

                    if (MyGameCard.HasChild && MyGameCard.Child.CardData.MyCardType is CardType.Humans)
                    {

                        if (!MyGameCard.TimerRunning)
                        {
                            MyGameCard.StartTimer(ReadingTime, GiveBlueprint, SokLoc.Translate("action_library_status"), GetActionId("GiveBlueprint")); // 
                            MyGameCard.CurrentTimerTime = timerTemp;
                        }
                    }
                    else
                    {
                        if (MyGameCard.TimerRunning)
                        {
                            timerTemp = MyGameCard.CurrentTimerTime;
                        }

                        MyGameCard.CancelTimer(GetActionId("GiveBlueprint"));
                    }
                }
            }
            if (AllBlueprintsFound())
            {
                MyGameCard.CancelTimer(GetActionId("GiveBlueprint"));
            }
            base.UpdateCard();
        }

        private bool AllBlueprintsFound()
        {
            bool result = true;
            foreach (string blueprintDrop in BlueprintDrops)
            {
                if (!WorldManager.instance.HasFoundCard(blueprintDrop))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        [TimedAction("give_blueprint")]
        public void GiveBlueprint()
        {
            foreach (string blueprintDrop in BlueprintDrops)
            {
                Blueprint blueprint = WorldManager.instance.GameDataLoader.GetCardFromId(blueprintDrop) as Blueprint;
                if ((bool)blueprint && !WorldManager.instance.HasFoundCard(blueprint.Id))
                {
                    CardData cardData2 = WorldManager.instance.CreateCard(MyGameCard.transform.position, blueprint, faceUp: true, checkAddToStack: false);
                    WorldManager.instance.CreateSmoke(cardData2.transform.position);
                    cardData2.MyGameCard.SendIt();

		            GameCard child = MyGameCard.Child;

		            if (!(child == null))
                    {
                        child.RemoveFromParent();
                        child.SendIt();
                    }

                    // AudioManager.me.PlaySound2D(InventionSound, 1f, 0.1f);

                    CoinCount = 0;
                    timerTemp = 0f;
                    break;
                }
            }
        }
    }

    
    public class CoinVault : Chest
    {
        public new void Awake()
        {
            MaxCoinCount = 500;
            ChestTerm = "mashlands_structure_coin_vault_description_long";
        }

        public override void Clicked()
        {
            int a = 10;
            if (CoinCount > 0)
            {
                int num = Mathf.Min(a, CoinCount);
                GameCard gameCard = WorldManager.instance.CreateCardStack(base.transform.position + Vector3.up * 0.2f, num, HeldCardId, checkAddToStack: false);
                WorldManager.instance.StackSend(gameCard.GetRootCard(), OutputDir, null, sendToChest: false);
                CoinCount -= num;
            }
            base.Clicked();
        }
    }


    public class Guillotine : CardData
    {
        public override bool DetermineCanHaveCardsWhenIsRoot => true;

        protected override bool CanHaveCard(CardData otherCard)
        {
            if (GetChildCount() <= 1 && otherCard.GetChildCount() == 0)
            {
                return otherCard.MyCardType == CardType.Humans;
            }
            return false;
        }


        public override void UpdateCard()
        {
            if (MyGameCard.HasChild && MyGameCard.Child.CardData.MyCardType is CardType.Humans)
            {
                MyGameCard.StartTimer(60f, ExecutingVillager, SokLoc.Translate("action_executing_villager_status"), GetActionId("ExecutingVillager"));
            }
            else
            {
                MyGameCard.CancelTimer(GetActionId("ExecutingVillager"));
            }
            base.UpdateCard();
        }

        [TimedAction("executing_villager")]
        public void ExecutingVillager()
        {
            if (MyGameCard.HasChild && MyGameCard.Child.CardData is BaseVillager)
            {
                WorldManager.instance.KillVillager(MyGameCard.Child.Combatable);
            }
        }

    }


    public class WanderingTrader : TravellingCart
    {
        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.MyGameCard == null)
            {
                return otherCard.Id == "gold";
            }
            if (WorldManager.instance.BoughtWithGold(otherCard.MyGameCard, GoldToUse, checkStackAllSame: true) || WorldManager.instance.BoughtWithGoldChest(otherCard.MyGameCard, GoldToUse))
            {
                return true;
            }
            if (otherCard.Id == "mashlands_resource_diamond")
            {
                return true;
            }
            return false;
        }

        public override void UpdateCard()
        {
            if (MyGameCard.HasChild)
            {
                GameCard child = MyGameCard.Child;
                if (WorldManager.instance.BoughtWithGold(child, GoldToUse))
                {
                    WorldManager.instance.RemoveCardsFromStackPred(child, GoldToUse, (GameCard x) => x.CardData.Id == "gold");
                    Buy();
                }
                else if (WorldManager.instance.BoughtWithGoldChest(child, GoldToUse))
                {
                    WorldManager.instance.BuyWithChest(child, GoldToUse);
                    Buy();
                }
                else if (child.CardData.Id == "mashlands_resource_diamond")
                {
                    WorldManager.instance.RemoveCardsFromStackPred(child, 1, (GameCard x) => x.CardData.Id == "mashlands_resource_diamond");
                    BuyDiamond();
                }
            }
            base.UpdateCard();
        }

        private void Buy()
        {
	    	ICardId cardId = MyCardBag.GetCard(removeCard: false);
		    WorldManager.instance.CreateCard(base.transform.position, cardId, faceUp: true, checkAddToStack: false).MyGameCard.SendIt();
	    	ItemsBought++;
        }

        private void BuyDiamond()
        {
	    	ICardId cardId = MyCardBag.GetCard(removeCard: false);

            int rand = UnityEngine.Random.Range(0,2);
            if (rand == 0)
            {
                cardId = (CardId)"goblet";
            }
            else if (rand == 1)
            {
                cardId = (CardId)"sacred_chest";
            }
            
		    WorldManager.instance.CreateCard(base.transform.position, cardId, faceUp: true, checkAddToStack: false).MyGameCard.SendIt();
	    	ItemsBought++;
        }
    }
}
