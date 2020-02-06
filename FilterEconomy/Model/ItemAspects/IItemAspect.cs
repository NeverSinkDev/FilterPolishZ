using FilterEconomy.Facades;
using FilterPolishUtil;
using FilterPolishUtil.Reflection;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace FilterEconomy.Model.ItemAspects
{
    public enum AspectType
    {
        common,
        uniques,
        divination,
        maps,
        basetype,
        fragments,
        prophecy,
        currency
    }

    public abstract class AbstractItemAspect : IItemAspect
    {
        static AbstractItemAspect()
        {
            var aspects = ReflectiveEnumerator.GetEnumerableOfType<AbstractItemAspect>().ToList();

            foreach (var item in aspects)
            {
                AvailableAspects.Add(item);
            }
        }

        public static AspectType RetrieveAspectType(string s)
        {
            if (s.ToLower().Contains("basetype"))
            {
                return AspectType.basetype;
            }

            if (s.ToLower().Contains("prophecy"))
            {
                return AspectType.prophecy;
            }

            switch (s)
            {
                case "currency":
                    return AspectType.currency;
                case "fragments":
                    return AspectType.fragments;
                case "uniques":
                    return AspectType.uniques;
                case "divination":
                    return AspectType.divination;
                case "maps":
                    return AspectType.maps;
                default:
                    return AspectType.common;
            }
        }

        public static ObservableCollection<AbstractItemAspect> AvailableAspects = new ObservableCollection<AbstractItemAspect>();

        public string Name => this.ToString().SubStringLast(".");
        public virtual string Group => "Ungrouped";
        public virtual AspectType Type => AspectType.common;

        public virtual bool IsActive()
        {
            return true;
        }
    }

    public interface IItemAspect
    {
        string Group { get; }
        string Name { get; }
        AspectType Type { get; }

        bool IsActive();
    }

    public class HandledAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public DateTime HandlingDate { get; set; }
        public float HanadlingPrice { get; set; }
    }

    public class IgnoreAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
    }

    public class AnchorAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
    }

    public class NonDropAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.uniques;
    }

    public class BossDropAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.uniques;
    }

    public class NonEventDropAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.uniques;
    }

    public class LeagueDropAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.uniques;
    }

    public class UncommonAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.uniques;
    }

    public class ChangedAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.uniques;
    }

    public class EarlyLeagueInterestAspect : AbstractItemAspect
    {
        public override string Group => "TemporalAspect";

        public override bool IsActive()
        {
            if (EconomyRequestFacade.GetInstance().ActiveMetaTags.ContainsKey("EarlyLeagueInterestAspect"))
            {
                if (EconomyRequestFacade.GetInstance().ActiveMetaTags["EarlyLeagueInterestAspect"] > DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class MetaBiasAspect : AbstractItemAspect
    {
        public override string Group => "TemporalAspect";
        public override bool IsActive()
        {
            if (EconomyRequestFacade.GetInstance().ActiveMetaTags.ContainsKey("MetaBiasAspect"))
            {
                if (EconomyRequestFacade.GetInstance().ActiveMetaTags["MetaBiasAspect"] > DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class ProphecyMaterialAspect : AbstractItemAspect
    {
        public override string Group => "Intent";
        public override AspectType Type => AspectType.uniques;
    }

    public class ProphecyResultAspect : AbstractItemAspect
    {
        public override string Group => "Intent";
        public override AspectType Type => AspectType.uniques;
    }

    public class HighVarietyAspect : AbstractItemAspect
    {
        public override string Group => "ItemProperties";
        public override AspectType Type => AspectType.uniques;
    }

    public class CurrencyTypeAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.divination;
    }

    public class PoorDiviAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.divination;
    }

    public class PreventHidingAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.divination;
    }

    public class LargeRandomPoolAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.divination;
    }

    public class QueryableResultAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.divination;
    }

    public class TimelessResultAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.divination;
    }

    public class SingleCardAspect : AbstractItemAspect
    {
        public override string Group => "StackSize";
        public override AspectType Type => AspectType.divination;
    }

    public class LargeDeckAspect : AbstractItemAspect
    {
        public override string Group => "StackSize";
        public override AspectType Type => AspectType.divination;
    }

    public class FarmableOrbAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.divination;
    }

    public class AtlasBaseAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.basetype;
    }

    public class SpecialImplicitAspect : AbstractItemAspect
    {
        public override string Group => "ItemProperties";
        public override AspectType Type => AspectType.basetype;
    }

    public class ExclusiveBaseAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
        public override AspectType Type => AspectType.basetype;
    }

    /// FRAGMENT ASPECTS

    public class TierableFragmentAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.fragments;
    }

    public class PredictableDropAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.fragments;
    }

    public class FloorFragmentsAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.fragments;
    }

    // CURRENCY ASPECT

    public class TierableCurrencyAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.currency;
    }

    public class ChaosBottomAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.currency;
    }

    public class AlchemyButtomAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.currency;
    }

    public class SilverBottomAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.currency;
    }

    public class ChanceBottomAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.currency;
    }

    // PROPHECY ASPECT

    public class MapUpgradeProphecyAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.prophecy;
    }

    public class LastInChainProphecyAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.prophecy;
    }

    public class ItemUpgradeProphecyAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.prophecy;
    }

    public class ItemDropProphecyAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.prophecy;
    }

    public class TimelessProphecyAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.prophecy;
    }

    public class CheapProphecyAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public override AspectType Type => AspectType.prophecy;
    }
}
