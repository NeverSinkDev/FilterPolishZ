using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Model.ItemAspects
{
    public abstract class AbstractItemAspect : IItemAspect
    {
        public string Name => this.ToString().SubStringLast(".");
        public virtual string Group => "Ungrouped";
    }

    public interface IItemAspect
    {
        string Group { get; }
        string Name { get; }
    }

    public class HandledAspect : AbstractItemAspect
    {
        public override string Group => "Meta";
        public DateTime HandlingDate { get; }
        public float HanadlingPrice { get; }
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
    }

    public class BossDropAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
    }

    public class LeagueDropAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
    }

    public class UncommonAspect : AbstractItemAspect
    {
        public override string Group => "DropType";
    }

    public class EarlyLeagueInterestAspect : AbstractItemAspect
    {
        public override string Group => "TemporalAspect";
    }

    public class MetaBiasAspect : AbstractItemAspect
    {
        public override string Group => "TemporalAspect";
    }

    public class ProphecyMaterialAspect : AbstractItemAspect
    {
        public override string Group => "Intent";
    }

    public class ProphecyResultAspect : AbstractItemAspect
    {
        public override string Group => "Intent";
    }

    public class HighVarietyAspect : AbstractItemAspect
    {
        public override string Group => "ItemProperties";
    }

    public class VarietyAspect : AbstractItemAspect
    {
        public override string Group => "ItemProperties";
    }
}
