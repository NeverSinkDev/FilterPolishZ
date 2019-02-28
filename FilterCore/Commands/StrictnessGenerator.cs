using System;
using System.Linq;
using FilterCore.Constants;
using FilterDomain.LineStrategy;

namespace FilterCore.Commands
{
    public class StrictnessGenerator
    {
        private readonly Filter filter;
        public int StrictnessLevel { get; }
        
        public StrictnessGenerator(Filter filter, int strictnessLevel)
        {
            this.filter = filter;
            this.StrictnessLevel = strictnessLevel;
        }

        public void Apply()
        {
            this.UpdateAllEntries();
        }

        private void UpdateAllEntries()
        {
            foreach (var entry in this.filter.FilterEntries)
            {
                if (entry.Header.Type != FilterConstants.FilterEntryType.Content)
                {
                    continue;
                }

                foreach (var tag in entry.Header.GenerationTags.Where(x => x.Strictness != -1).ToList())
                {
                    if (tag.Strictness >= FilterConstants.FilterStrictnessLevels.Count-1)
                    {
                        throw new Exception("invalid strictness level");
                    }
                    
                    // tag = 1 means: skip if the strictness is 1 or lower
                    if (tag.Strictness >= this.StrictnessLevel)
                    {
                        continue;
                    }

                    switch (tag.Value.ToLower())
                    {
                        case "d":
                            entry.Header.IsFrozen = true;
                            break;

                        case "h":
                            entry.Header.HeaderValue = "Hide";
                            break;
                    
                        case "rf": // reduce fontSize to 36
                            var line = entry.Content.GetFirst("SetFontSize");
                            if (line == null) continue;
                            var val = (VariableValueContainer) line.Value;
                            val.Value.First().value = "36";
                            break;
                    
                        case "hs": // same as "rems", but also hide on the next strictness after that
                            if (tag.Strictness+1 < this.StrictnessLevel)
                            {
                                entry.Header.HeaderValue = "Hide";
                            }
                            goto case "rems";
                        
                        case "rems": // remove highlights (sound, beam, icon)
                            FilterConstants.HighlightingIdents.ToList().ForEach(x => entry.Content.RemoveAll(x));
                            break;

                        default:
                            throw new Exception("unknown strictness generation tag: " + tag.Value);
                    }
                }
            }
        }
    }
}