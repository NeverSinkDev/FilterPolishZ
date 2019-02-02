using FilterDomain.LineStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterDomain
{
    public class FilterConstants
    {
        public static Dictionary<string, ILineStrategy> LineTypes = new Dictionary<string, ILineStrategy>(){
            { "ShapedMap",     new BoolLineStrategy() },
            { "ElderItem",     new BoolLineStrategy() },
            { "ShaperItem",    new BoolLineStrategy() },
            { "Corrupted",     new BoolLineStrategy() },
            { "Identified",    new BoolLineStrategy() },
            { "ElderMap",      new BoolLineStrategy() },
            { "LinkedSockets", new NumericLineStrategy() },
            { "Sockets",       new NumericLineStrategy() },
            { "Quality",       new NumericLineStrategy() },
            { "MapTier",       new NumericLineStrategy() },
            { "Width",         new NumericLineStrategy() },
            { "Height",        new NumericLineStrategy() },
            { "StackSize",     new NumericLineStrategy() },
            { "GemLevel",      new NumericLineStrategy() },
            { "ItemLevel",     new NumericLineStrategy() },
            { "DropLevel",     new NumericLineStrategy() },
            { "Rarity",        new EnumLineStrategy() },
            { "SocketGroup",   new EnumLineStrategy() },
            { "Class",         new EnumLineStrategy() },
            { "BaseType",      new EnumLineStrategy() },
            { "Prophecy",      new EnumLineStrategy() },
            { "HasExplicitMod",new EnumLineStrategy() },

            { "SetFontSize",             new VariableLineStrategy(1,1) },
            { "SetTextColor",            new ColorLineStrategy() },
            { "SetBorderColor",          new ColorLineStrategy() },
            { "SetBackgroundColor",      new ColorLineStrategy() },
            { "PlayAlertSound",          new VariableLineStrategy(1,2) },
            { "PlayAlertSoundPositional",new VariableLineStrategy(1,2) },
            { "CustomAlertSound",        new VariableLineStrategy(1,2) },
            { "DisableDropSound",        new VariableLineStrategy(0,1) },
            { "MinimapIcon",             new VariableLineStrategy(3,3) },
            { "PlayEffect",              new VariableLineStrategy(2,3) }};

        public enum LineAttributeVisual
        {
            Unknown,
            SetFontSize,
            SetTextColor,
            SetBorderColor,
            SetBackgroundColor,
            PlayAlertSound,
            CustomAlertSound,
            DisableDropSound,
            MinimapIcon,
            PlayEffect
        }

        public enum LineAttributeClass
        {
            Unknown,
            ShapedMap,
            ElderItem,
            ShaperItem,
            Corrupted,
            Identified,
            ElderMap,
            LinkedSockets,
            Sockets,
            Quality,
            MapTier,
            Width,
            Height,
            StackSize,
            GemLevel,
            ItemLevel,
            DropLevel,
            Rarity,
            SocketGroup,
            Class,
            BaseType,
            HasExplicitMod
        }
    }
}
