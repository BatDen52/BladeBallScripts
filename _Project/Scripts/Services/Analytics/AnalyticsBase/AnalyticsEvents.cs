namespace _Project
{
    public static class AnalyticsEvents
    {
        public const string game_start = "game_start"; // Sent at the start of the game, at the moment of application initialization
        // count	int	total number of started sessions
        
        public const string total_playtime = "total_playtime"; // Sent every minute of total time player spent in a game since the first launch 
        // minutes_total int total amount of time spent in the game at the moment of event in minutes
        
        public const string level_start = "level_start"; // Sent every time user starts the level. Even if the level was restarted after fail or by user
        // level
        
        public const string level_complete = "level_complete"; // Sent at the end of the level. 
        // level
        // time_spent
        
        public const string level_fail = "level_fail"; // Sent at the moment of level failure
        // level
        // time_spent

        
        public const string currency_spent = "currency_spent"; // Sent at the moment of spending soft currency
        // currency	string	"the  name of spent currency: gold, gems, dollars, etc..."
        // type	string	"type of spending: upgrade, shop purchase, etc..."
        // item	string	"the name of the specific item: upgrade_lvl_1, product name, etc.)"
        // price	int	amount of currency spent
        // count	int	total number of soft_spent events
        
        public const string currency_gained = "currency_gained"; // Sent at the moment player gains currency
        // currency	string	"the  name of spent currency: gold, gems, dollars, etc..."
        // type	string	"type of spending: upgrade, shop purchase, etc..."
        // item	string	"the name of the specific item: upgrade_lvl_1, product name, etc.)"
        // price	int	amount of currency spent
        // count	int	total number of soft_spent events


        public const string video_ads_triggered = "video_ads_triggered"; // Sent when the ad show triggered (automatically or by user) 
        // "The event indicates if the triggered ad is available to start (whether it is loaded on the device)
        //The fact that the ad itself is loaded does not need to be tracked, it can be loaded in the background.    
        //Result = fail if the ad show was triggered, but the ad was not ready to show "
        // ad_type string ad_type: interstitial, rewarded
        // placement string	name of the placement
        // result string success, fail
        // internet bool "internet connection status at the moment of the event 1 = connected 0 = disconnected"
        // ad_network string "the name of the advertising network providing the ad. ad_network: unity, applovin, facebook, etc..."

        public const string video_ads_started = "video_ads_started"; // Sent when the video ad started
        // "The event indicates if the triggered ad has been started
        // The event could only be dispatched after the event video_ads_triggered
        // Only one event may be generated per one ad impression
        // The best option is to send the event immediately after ad start"
        // ad_type string ad_type: interstitial, rewarded
        // placement string	name of the placement
        // result string start
        // internet bool "internet connection status at the moment of the event 1 = connected 0 = disconnected"
        // ad_network string "the name of the advertising network providing the ad. ad_network: unity, applovin, facebook, etc..."
        
        public const string video_ads_complete = "video_ads_complete"; // Sent when the video ad completed
        // "The event is sent after the end of the video ad"
        // ad_type string ad_type: interstitial, rewarded
        // placement string	name of the placement
        // result watched, clicked, canceled
        // internet bool "internet connection status at the moment of the event 1 = connected 0 = disconnected"
        // ad_network string "the name of the advertising network providing the ad. ad_network: unity, applovin, facebook, etc..."
        
        public const string ad_impression = "ad_impression";
        
        public const string load_scene_start = "load_scene_start"; 
        public const string load_scene_complete = "load_scene_complete";    
        public const string load_first_scene_start = "load_first_scene_start"; 
        public const string load_first_scene_complete = "load_first_scene_complete";
        // name string name of scene
        
        public const string portal_enter = "portal_enter";

        public const string tutorial_start = "tutorial_start";
        public const string tutorial_complete = "tutorial_complete";
        public const string tutorial_failed = "tutorial_failed";
        
        public const string death = "death";
        public const string death_accident = "death_accident";
        public const string kill = "kill";
        
        public const string first_block = "first_block";
        
        public const string skill_select = "skill_select"; // string name, bool is_owned
        public const string skill_use = "skill_use";
        public const string skill_buy = "skill_buy";
        
        public const string weapon_select = "weapon_select"; // string name, bool is_owned
        public const string skin_select = "skin_select"; // string name, bool is_owned
        
        public const string open_window = "open_window";
    }
}