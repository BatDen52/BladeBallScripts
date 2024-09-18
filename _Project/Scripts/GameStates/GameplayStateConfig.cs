namespace _Project
{

    public class GameplayStateConfig
    {
        public enum BattleKind
        {
            CLASSIC,
            DUEL_1_1
        };

        public BattleKind   Kind;
        public int          ScoreToWin;

        public static GameplayStateConfig CreateClassicConfig()
        {
            var result = new GameplayStateConfig()
            {
                Kind = BattleKind.CLASSIC,
                ScoreToWin = 1,
            };
            return result;
        }

        public static GameplayStateConfig CreateDuel_1_1Config()
        {
            var result = new GameplayStateConfig()
            {
                Kind = BattleKind.DUEL_1_1,
                ScoreToWin = 3,
            };
            return result;
        }
    }
}
