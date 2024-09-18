namespace _Project
{
    public static class AnalyticsParameters
    {
        public const string total_playtime_min = "total_playtime_min"; // int | total time spent in the game from the moment of installation to the moment of event generation in minutes
        public const string total_playtime_sec = "total_playtime_sec"; // int | total time spent in the game from the moment of installation to the moment of event generation in seconds
        public const string session_playtime_sec = "session_playtime_sec"; // int | time spent in the game in current session in seconds
        public const string days_in_game = "days_in_game"; // int | Total amount of days passed since registration
        public const string session_number = "session_number"; // int | Number of current session
        public const string user_level = "user_level"; // int | Current user level
        public const string last_level = "last_level"; // int | Last finished level
        public const string level = "level"; // int | level number
        public const string mode = "mode"; // string | deathmatch, duel, bossfight, tutorial etc
        public const string time_spent = "time_spent"; // int | time spent on level before completion (in seconds)
        public const string currency = "currency"; // string | the  name of spent currency: gold, gems
        public const string type = "type"; // string | type of spending: buy_skill, open_chest
        public const string item = "item"; // string | the name of the specific item: platform, superjump, weapon, weapon_premium, etc
        public const string price = "price"; // int | amount of currency spent
        public const string count = "count"; // int
        public const string source = "source"; // string | source of income: rewarded_shop, deathmatch
        public const string amount = "amount"; // int | amount of currency gained
        public const string minutes_total = "minutes_total"; // int | total amount of time spent in the game at the moment of event in minutes
        public const string ad_type = "ad_type"; // string | interstitial, rewarded
        public const string placement = "placement"; // string | name of the placement: shop, deathmatch
        public const string result = "result"; // string | success, fail
        public const string internet = "internet"; // bool | internet connection status at the moment of the event 1 = connected 0 = disconnected
        public const string ad_network = "ad_network"; // string | the name of the advertising network providing the ad. ad_network: unity, applovin, facebook, etc
        public const string is_owned = "is_owned"; // bool | is owned 1 = owned 0 = not owned
        public const string name = "name"; // string | name of skill, weapon etc
        public const string scene = "scene"; // string | name of scene
        
        public const string value = "value";
        
        public const string ad_platform = "ad_platform";
        public const string ad_source = "ad_source";
        public const string ad_unit_name = "ad_unit_name";
        public const string ad_format = "ad_format";
    }
}