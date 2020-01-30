namespace FilterEconomy.Model
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class PoeLeague
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("realm")]
        public string Realm { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("registerAt")]
        public DateTimeOffset RegisterAt { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("startAt")]
        public DateTimeOffset StartAt { get; set; }

        [JsonProperty("endAt")]
        public DateTimeOffset? EndAt { get; set; }

        [JsonProperty("delveEvent")]
        public bool DelveEvent { get; set; }

        [JsonProperty("rules")]
        public List<Rule> Rules { get; set; }

        [JsonProperty("timedEvent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TimedEvent { get; set; }

        [JsonProperty("scoreEvent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ScoreEvent { get; set; }
    }

    public partial class Rule
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
